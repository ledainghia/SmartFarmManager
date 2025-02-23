using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.BusinessModels.FarmingBatch;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.Configuration;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Shared;
using Sprache;

namespace SmartFarmManager.Service.Services
{
    public class FarmingBatchService : IFarmingBatchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskService _taskService;

        public FarmingBatchService(IUnitOfWork unitOfWork, ITaskService taskService)
        {
            _unitOfWork = unitOfWork;
            _taskService = taskService;
        }

        public async Task<bool> CreateFarmingBatchAsync(CreateFarmingBatchModel model)
        {
            var configService = new SystemConfigurationService();
            var config = await configService.GetConfigurationAsync();

            // Kiểm tra số lần tạo vụ nuôi trong chuồng
            var batchCount = await _unitOfWork.FarmingBatches
        .FindByCondition(fb => fb.CageId == model.CageId &&
                               (fb.Status == FarmingBatchStatusEnum.Planning ||
                                fb.Status == FarmingBatchStatusEnum.Active))
        .CountAsync();
            if (batchCount >= config.MaxFarmingBatchPerCage)
            {
                throw new InvalidOperationException($"Chuồng này đã đạt số lượng vụ nuôi tối đa ({config.MaxFarmingBatchPerCage}).");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {

                var animalTemplate = await _unitOfWork.AnimalTemplates
                    .FindByCondition(a => a.Id == model.TemplateId && a.Status == "Active")
                    .Include(a => a.GrowthStageTemplates)
                    .ThenInclude(gst => gst.TaskDailyTemplates)
                    .Include(a => a.GrowthStageTemplates)
                    .ThenInclude(gst => gst.FoodTemplates)
                    .Include(a => a.VaccineTemplates)
                    .FirstOrDefaultAsync();

                if (animalTemplate == null)
                {
                    throw new ArgumentException($"Animal template with ID {model.TemplateId} does not exist or is inactive.");
                }

                var cage = await _unitOfWork.Cages.FindAsync(x => x.Id == model.CageId && !x.IsDeleted);
                if (cage == null)
                {
                    throw new ArgumentException($"Cage with ID {model.CageId} does not exist or is inactive.");
                }

                var farmingBatch = new FarmingBatch
                {
                    TemplateId = model.TemplateId,
                    CageId = model.CageId,
                    Name = model.Name,
                    CleaningFrequency = model.CleaningFrequency,
                    Quantity = model.Quantity,
                    FarmId = cage.FarmId,
                    Status = FarmingBatchStatusEnum.Planning,
                    StartDate = null // StartDate sẽ được cập nhật sau khi chuyển trạng thái
                };

                await _unitOfWork.FarmingBatches.CreateAsync(farmingBatch);
                await _unitOfWork.CommitAsync();

                var growthStages = animalTemplate.GrowthStageTemplates
                    .Select(template => new GrowthStage
                    {
                        FarmingBatchId = farmingBatch.Id,
                        Name = template.StageName,
                        WeightAnimal = template.WeightAnimal,
                        AgeStart = template.AgeStart,
                        AgeEnd = template.AgeEnd,
                        FoodType = template.FoodTemplates.FirstOrDefault()?.FoodType,
                        Status = GrowthStageStatusEnum.Planning,
                        Quantity=model.Quantity,
                        AgeStartDate = null, // Sẽ được cập nhật khi trạng thái chuyển sang Active
                        AgeEndDate = null,
                        SaleTypeId= template.SaleTypeId,
                        RecommendedWeightPerSession = farmingBatch.Quantity * (template.WeightAnimal ?? 0) * (template.FoodTemplates.Sum(f => f.WeightBasedOnBodyMass) ?? 0),
                        WeightBasedOnBodyMass = template.FoodTemplates.Sum(f => f.WeightBasedOnBodyMass)
                    }).ToList();

                await _unitOfWork.GrowthStages.CreateListAsync(growthStages);
                await _unitOfWork.CommitAsync();

                var taskDailyList = growthStages
                    .SelectMany(stage => animalTemplate.GrowthStageTemplates
                        .Where(template =>
                            template.StageName == stage.Name &&
                            template.AgeStart == stage.AgeStart &&
                            template.AgeEnd == stage.AgeEnd)
                        .SelectMany(template => template.TaskDailyTemplates.Select(taskTemplate => new TaskDaily
                        {
                            GrowthStageId = stage.Id,
                            TaskTypeId = taskTemplate.TaskTypeId,
                            TaskName = taskTemplate.TaskName,
                            Description = taskTemplate.Description,
                            Session = taskTemplate.Session,
                            StartAt = null, // Sẽ được cập nhật sau
                            EndAt = null
                        })))
                    .ToList();

                await _unitOfWork.TaskDailies.CreateListAsync(taskDailyList);

                var vaccines = await _unitOfWork.Vaccines
                    .FindByCondition(v => animalTemplate.VaccineTemplates.Select(vt => vt.VaccineName).Contains(v.Name))
                    .ToListAsync();

                var vaccineSchedules = new List<VaccineSchedule>();
                foreach (var vaccineTemplate in animalTemplate.VaccineTemplates)
                {
                    var vaccine = vaccines.FirstOrDefault(v => v.Name == vaccineTemplate.VaccineName);

                    if (vaccine == null)
                    {
                        throw new ArgumentException($"Vaccine with name '{vaccineTemplate.VaccineName}' does not exist.");
                    }

                    var applicableGrowthStage = growthStages.FirstOrDefault(gs =>
                        gs.AgeStart <= vaccineTemplate.ApplicationAge && gs.AgeEnd >= vaccineTemplate.ApplicationAge);

                    if (applicableGrowthStage != null)
                    {
                        vaccineSchedules.Add(new VaccineSchedule
                        {
                            StageId = applicableGrowthStage.Id,
                            VaccineId = vaccine.Id,
                            Quantity = farmingBatch.Quantity,
                            ApplicationAge = vaccineTemplate.ApplicationAge,
                            Session=vaccineTemplate.Session,
                            Status = VaccineScheduleStatusEnum.Upcoming,
                            Date = null // Ngày sẽ được cập nhật khi trạng thái chuyển sang Active
                        });
                    }
                }

                await _unitOfWork.VaccineSchedules.CreateListAsync(vaccineSchedules);

                await _unitOfWork.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                Console.WriteLine($"Error in CreateFarmingBatchAsync: {ex.Message}");
                throw new Exception("Failed to create Farming Batch. Details: " + ex.Message);
            }
        }




        public async Task<bool> UpdateFarmingBatchStatusAsync(Guid farmingBatchId, string newStatus)
        {
            // Lấy thông tin FarmingBatch
            var farmingBatch = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.Id == farmingBatchId)
                .Include(fb => fb.GrowthStages)
                .ThenInclude(gs => gs.VaccineSchedules)
                .Include(fb => fb.GrowthStages)
                .ThenInclude(gs => gs.TaskDailies)
                .Include(fb => fb.MedicalSymptoms)
                .ThenInclude(ms => ms.Prescriptions)
                .FirstOrDefaultAsync();

            if (farmingBatch == null)
            {
                throw new ArgumentException($"FarmingBatch với ID {farmingBatchId} không tồn tại.");
            }

            // Kiểm tra trạng thái hiện tại
            if (farmingBatch.Status == newStatus)
            {
                throw new InvalidOperationException($"FarmingBatch đã ở trạng thái '{newStatus}'.");
            }

            // Kiểm tra trạng thái hợp lệ
            if (newStatus != FarmingBatchStatusEnum.Active &&
                newStatus != FarmingBatchStatusEnum.Completed &&
                newStatus != FarmingBatchStatusEnum.Cancelled)
            {
                throw new ArgumentException($"Trạng thái chuyển đổi '{newStatus}' không hợp lệ.");
            }

            if (farmingBatch.Status == FarmingBatchStatusEnum.Planning && newStatus == FarmingBatchStatusEnum.Active)
            {
                // **Kiểm tra xem chuồng này có FarmingBatch nào đang hoạt động không**
                var activeBatchExists = await _unitOfWork.FarmingBatches
                    .FindByCondition(fb => fb.CageId == farmingBatch.CageId && fb.Status == FarmingBatchStatusEnum.Active)
                    .AnyAsync();

                if (activeBatchExists)
                {
                    throw new InvalidOperationException($"Chuồng này đã có một FarmingBatch đang hoạt động. Không thể kích hoạt thêm.");
                }

                // **Chuyển trạng thái sang Active**
                farmingBatch.Status = FarmingBatchStatusEnum.Active;
                farmingBatch.StartDate = DateTimeUtils.GetServerTimeInVietnamTime();

                var currentStartDate = farmingBatch.StartDate;

                // **Cập nhật GrowthStages**
                foreach (var stage in farmingBatch.GrowthStages.OrderBy(gs => gs.AgeStart))
                {
                    stage.AgeStartDate = currentStartDate;
                    stage.AgeEndDate = currentStartDate.Value.AddDays(stage.AgeEnd - stage.AgeStart ?? 0);

                    stage.Status = stage.AgeStartDate == farmingBatch.StartDate
                        ? GrowthStageStatusEnum.Active
                        : GrowthStageStatusEnum.Upcoming;

                    currentStartDate = stage.AgeEndDate.Value.AddDays(1);

                    // **Cập nhật TaskDaily**
                    foreach (var taskDaily in stage.TaskDailies)
                    {
                        taskDaily.StartAt = stage.AgeStartDate;
                        taskDaily.EndAt = stage.AgeEndDate;
                        await _unitOfWork.TaskDailies.UpdateAsync(taskDaily);
                    }

                    // **Cập nhật VaccineSchedule**
                    foreach (var vaccineSchedule in stage.VaccineSchedules)
                    {
                        if (stage.AgeStartDate.HasValue)
                        {
                            vaccineSchedule.Date = stage.AgeStartDate.Value.AddDays(
                                (vaccineSchedule.ApplicationAge ?? 0) - (stage.AgeStart ?? 0)
                            );

                            if (vaccineSchedule.Date > DateTimeUtils.GetServerTimeInVietnamTime().Date)
                            {
                                vaccineSchedule.Status = VaccineScheduleStatusEnum.Upcoming;
                            }
                            else if (vaccineSchedule.Date == DateTimeUtils.GetServerTimeInVietnamTime().Date)
                            {
                                vaccineSchedule.Status = VaccineScheduleStatusEnum.Completed;
                            }
                            else
                            {
                                vaccineSchedule.Status = VaccineScheduleStatusEnum.Missed;
                            }

                            await _unitOfWork.VaccineSchedules.UpdateAsync(vaccineSchedule);
                        }
                    }

                    await _unitOfWork.GrowthStages.UpdateAsync(stage);
                }

                // Lưu thay đổi
                await _unitOfWork.FarmingBatches.UpdateAsync(farmingBatch);
                await _unitOfWork.CommitAsync();

                // Gọi hàm generate task
                await _taskService.GenerateTasksForFarmingBatchAsync(farmingBatchId);
            }
            else if (newStatus == FarmingBatchStatusEnum.Completed)
            {
                // **Chuyển trạng thái sang Completed**
                farmingBatch.Status = FarmingBatchStatusEnum.Completed;
                farmingBatch.CompleteAt = DateTimeUtils.GetServerTimeInVietnamTime();

                foreach (var stage in farmingBatch.GrowthStages)
                {
                    stage.Status = GrowthStageStatusEnum.Completed;

                    foreach (var vaccineSchedule in stage.VaccineSchedules)
                    {
                        if (vaccineSchedule.Status == VaccineScheduleStatusEnum.Upcoming)
                        {
                            vaccineSchedule.Status = VaccineScheduleStatusEnum.Missed;
                            await _unitOfWork.VaccineSchedules.UpdateAsync(vaccineSchedule);
                        }
                    }

                    await _unitOfWork.GrowthStages.UpdateAsync(stage);
                }

                // Lưu thay đổi
                await _unitOfWork.FarmingBatches.UpdateAsync(farmingBatch);
                await _unitOfWork.CommitAsync();
            }
            else if (newStatus == FarmingBatchStatusEnum.Cancelled)
            {
                // **Chuyển trạng thái sang Cancelled**
                farmingBatch.Status = FarmingBatchStatusEnum.Cancelled;

                // **Hủy các Prescription liên quan đến MedicalSymptoms**
                foreach (var medicalSymptom in farmingBatch.MedicalSymptoms)
                {
                    foreach (var prescription in medicalSymptom.Prescriptions)
                    {
                        if (prescription.Status == PrescriptionStatusEnum.Active)
                        {
                            prescription.Status = PrescriptionStatusEnum.Cancelled;
                            await _unitOfWork.Prescription.UpdateAsync(prescription);
                        }
                    }
                }

                // **Cập nhật trạng thái GrowthStages và VaccineSchedules**
                foreach (var stage in farmingBatch.GrowthStages)
                {
                    stage.Status = GrowthStageStatusEnum.Cancelled;

                    foreach (var vaccineSchedule in stage.VaccineSchedules)
                    {
                        if (vaccineSchedule.Status == VaccineScheduleStatusEnum.Upcoming)
                        {
                            vaccineSchedule.Status = VaccineScheduleStatusEnum.Cancelled;
                            await _unitOfWork.VaccineSchedules.UpdateAsync(vaccineSchedule);
                        }
                    }

                    await _unitOfWork.GrowthStages.UpdateAsync(stage);
                }

                // Lưu thay đổi
                await _unitOfWork.FarmingBatches.UpdateAsync(farmingBatch);
                await _unitOfWork.CommitAsync();
            }

            return true;
        }


        public async Task<PagedResult<FarmingBatchModel>> GetFarmingBatchesAsync(string? cageName, string? name, string? species, DateTime? startDateFrom, DateTime? startDateTo, int pageNumber, int pageSize, Guid? cageId, bool? isCancel)
        {
            var query = _unitOfWork.FarmingBatches.FindAll()
                .Include(fb => fb.Cage) // Include related Cage
                .Include(fb => fb.Template)
                .AsQueryable();

            // Apply Filters
            if (!isCancel.Value)
            {
                query = query.Where(x => x.Status != FarmingBatchStatusEnum.Cancelled);
            }

            if (!string.IsNullOrEmpty(cageName))
            {
                query = query.Where(x => x.Cage.Name.Contains(cageName));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name.Contains(name));
            }

            if (startDateFrom.HasValue)
            {
                query = query.Where(x => x.StartDate >= startDateFrom.Value);
            }

            if (startDateTo.HasValue)
            {
                query = query.Where(x => x.StartDate <= startDateTo.Value);
            }
            if (cageId.HasValue)
            {
                query = query.Where(x => x.CageId == cageId);
            }

            // Pagination
            var totalItems = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(fb => new FarmingBatchModel
                {
                    Id = fb.Id,
                    Name = fb.Name,
                    StartDate = fb.StartDate,
                    CompleteAt = fb.CompleteAt,
                    Status = fb.Status,
                    CleaningFrequency = fb.CleaningFrequency,
                    Quantity = fb.Quantity,
                    Cage = fb.Cage == null ? null : new CageModel
                    {
                        Id = fb.Cage.Id,
                        Name = fb.Cage.Name,
                        Capacity = fb.Cage.Capacity,
                        FarmId = fb.Cage.FarmId,
                        Location = fb.Cage.Location,
                        Area = fb.Cage.Area

                    },
                    Template = fb.Template == null ? null : new BusinessModels.AnimalTemplate.AnimalTemplateItemModel
                    {
                        Id = fb.Template.Id,
                        Name = fb.Template.Name,
                        Species = fb.Template.Species,
                        Status = fb.Template.Status,
                        Notes = fb.Template.Notes
                    }
                })
    .ToListAsync();

            var resultPaging = new PaginatedList<FarmingBatchModel>(items, totalItems, pageNumber, pageSize);
            return new PagedResult<FarmingBatchModel>
            {
                Items = resultPaging.Items,
                TotalItems = resultPaging.TotalCount,
                PageSize = resultPaging.PageSize,
                CurrentPage = resultPaging.CurrentPage,
                TotalPages = resultPaging.TotalPages,
                HasNextPage = resultPaging.HasNextPage,
                HasPreviousPage = resultPaging.HasPreviousPage
            };

        }

        public async Task<FarmingBatchModel> GetActiveFarmingBatchByCageIdAsync(Guid cageId)
        {
            var currentDate = DateOnly.FromDateTime(DateTimeUtils.GetServerTimeInVietnamTime());

            // Tìm FarmingBatch theo CageId và các điều kiện
            var farmingBatch = await _unitOfWork.FarmingBatches.FindByCondition(
                fb => fb.CageId == cageId
                      && fb.Status == FarmingBatchStatusEnum.Active
                      && fb.StartDate.HasValue
                      && DateOnly.FromDateTime(fb.StartDate.Value) <= currentDate,
                trackChanges: false
            ).FirstOrDefaultAsync();

            if (farmingBatch == null)
                return null;

            return new FarmingBatchModel
            {
                Id = farmingBatch.Id,
                Name = farmingBatch.Name,
                StartDate = farmingBatch.StartDate,
                CompleteAt = farmingBatch.CompleteAt,
                Status = farmingBatch.Status,
                CleaningFrequency = farmingBatch.CleaningFrequency,
                Quantity = farmingBatch.Quantity,
            };
        }

        public async Task<List<FarmingBatchModel>> GetActiveFarmingBatchesByUserAsync(Guid userId)
        {
            // Lấy danh sách Cage theo userId
            var cages = await _unitOfWork.Cages
                .FindByCondition(c => c.CageStaffs.Any(cs => cs.StaffFarmId == userId) && !c.IsDeleted && c.BoardStatus)
                .Include(c => c.FarmingBatches)
                .ToListAsync();

            // Lấy danh sách tất cả FarmingBatches từ các Cage
            var farmingBatches = cages
                .SelectMany(c => c.FarmingBatches)
                .Where(fb => fb.Status == FarmingBatchStatusEnum.Active) // Lọc chỉ lấy các FarmingBatch Active
                .ToList();

            // Map sang FarmingBatchModel
            return farmingBatches.Select(fb => new FarmingBatchModel
            {
                Id = fb.Id,
                Name = fb.Name,
                StartDate = fb.StartDate,
                CompleteAt = fb.CompleteAt,
                EndDate = fb.EndDate,
                Status = fb.Status,
                CleaningFrequency = fb.CleaningFrequency,
                Quantity = fb.Quantity,
                AffectedQuantity = fb.AffectedQuantity,
            }).ToList();
        }

        public async Task<FarmingBatchReportResponse> GetFarmingBatchReportAsync(Guid farmingBatchId)
        {
            var farmingBatch = await _unitOfWork.FarmingBatches
                .FindAll()
                .Where(fb => fb.Id == farmingBatchId && fb.Status == FarmingBatchStatusEnum.Completed)
                .Include(fb => fb.AnimalSales)
                .Include(fb => fb.GrowthStages)
                    .ThenInclude(gs => gs.DailyFoodUsageLogs)
                .Include(fb => fb.GrowthStages)
                    .ThenInclude(gs => gs.VaccineSchedules)
                .Include(fb => fb.MedicalSymptoms)
                    .ThenInclude(ms => ms.Prescriptions)
                .FirstOrDefaultAsync();

            if (farmingBatch == null)
                return null;

            // Tổng tiền bán trứng (SaleType = "EggSale")
            var totalEggSales = farmingBatch.AnimalSales
                .Where(sale => sale.SaleType.StageTypeName == "EggSale")
                .Sum(sale => sale.UnitPrice * sale.Quantity) ?? 0;

            // Tổng tiền bán thịt (SaleType = "MeatSale")
            var totalMeatSales = farmingBatch.AnimalSales
                .Where(sale => sale.SaleType.StageTypeName == "MeatSale")
                .Sum(sale => sale.UnitPrice * sale.Quantity) ?? 0;

            // Tổng tiền thức ăn (tất cả các GrowthStage)
            var totalFoodCost = farmingBatch.GrowthStages
                .SelectMany(gs => gs.DailyFoodUsageLogs)
                .Sum(log => (decimal)log.UnitPrice * (log.ActualWeight ?? 0));

            // Tổng tiền vaccine (tất cả các GrowthStage)
            var totalVaccineCost = farmingBatch.GrowthStages
                .SelectMany(gs => gs.VaccineSchedules)
                .Sum(vaccine => vaccine.Quantity * (vaccine.ToltalPrice ?? 0));

            // Tổng tiền thuốc (từ tất cả các MedicalSymptom và Prescription)
            var totalMedicineCost = farmingBatch.MedicalSymptoms
                .SelectMany(ms => ms.Prescriptions)
                .Sum(p => p.Price ?? 0);

            // Lợi nhuận: Tổng doanh thu - Tổng chi phí
            var netProfit = ((decimal)totalEggSales + (decimal)totalMeatSales) - (totalFoodCost + (decimal)totalVaccineCost + totalMedicineCost);

            return new FarmingBatchReportResponse
            {
                FarmingBatchId = farmingBatch.Id,
                FarmingBatchName = farmingBatch.Name,
                StartDate = farmingBatch.StartDate,
                EndDate = farmingBatch.CompleteAt,
                TotalEggSales = (decimal)totalEggSales,
                TotalMeatSales = (decimal)totalMeatSales,
                TotalFoodCost = totalFoodCost,
                TotalVaccineCost = (decimal)totalVaccineCost,
                TotalMedicineCost = totalMedicineCost,
                NetProfit = netProfit
            };
        }

    }
}
