using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.BusinessModels.DailyFoodUsageLog;
using SmartFarmManager.Service.BusinessModels.FarmingBatch;
using SmartFarmManager.Service.BusinessModels.Prescription;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.BusinessModels.Vaccine;
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
            var farmConfig = await _unitOfWork.FarmConfigs.FindAll().FirstOrDefaultAsync();

            // Kiểm tra số lần tạo vụ nuôi trong chuồng
            var batchCount = await _unitOfWork.FarmingBatches
        .FindByCondition(fb => fb.CageId == model.CageId &&
                               (fb.Status == FarmingBatchStatusEnum.Planning ||
                                fb.Status == FarmingBatchStatusEnum.Active))
        .CountAsync();
            if (batchCount >= farmConfig.MaxFarmingBatchesPerCage)
            {
                throw new InvalidOperationException($"Chuồng này đã đạt số lượng vụ nuôi tối đa ({farmConfig.MaxFarmingBatchesPerCage}).");
            }

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

            var ageEndMax = animalTemplate.GrowthStageTemplates.Max(gst => gst.AgeEnd);
            var estimatedTimeEnd = model.EstimatedTimeStart.Value.AddDays(ageEndMax ?? 0);  

            var existingBatch = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.CageId == model.CageId &&
                                        fb.Status != FarmingBatchStatusEnum.Completed &&
                                        fb.EstimatedTimeStart.HasValue &&
                                        fb.EndDate.HasValue &&
                                        (
                                            // Vụ nuôi mới bắt đầu trước và kết thúc sau vụ nuôi cũ
                                            (fb.EstimatedTimeStart.Value.Date <= model.EstimatedTimeStart.Value.Date && fb.EndDate.Value.Date >= estimatedTimeEnd.Date) ||

                                            // Vụ nuôi mới bắt đầu trong khoảng thời gian vụ nuôi cũ
                                            (fb.EstimatedTimeStart.Value.Date <= model.EstimatedTimeStart.Value.Date && fb.EndDate.Value.Date >= model.EstimatedTimeStart.Value.Date) ||

                                            // Vụ nuôi mới kết thúc trong khoảng thời gian vụ nuôi cũ
                                            (fb.EstimatedTimeStart.Value.Date <= estimatedTimeEnd.Date && fb.EndDate.Value.Date >= estimatedTimeEnd.Date)
                                        ))
                .FirstOrDefaultAsync();
            if (existingBatch != null)
            {
                throw new InvalidOperationException($"Đã có vụ nuôi khác trong khoảng thời gian đã chọn.");
            }



            await _unitOfWork.BeginTransactionAsync();

            try
            {

               

                var cage = await _unitOfWork.Cages.FindAsync(x => x.Id == model.CageId && !x.IsDeleted);
                if (cage == null)
                {
                    throw new ArgumentException($"Cage with ID {model.CageId} does not exist or is inactive.");
                }

                var farmingBatch = new FarmingBatch
                {
                    FarmingBatchCode = GenerateFarmingBatchCode(model.TemplateId,(DateTime)model.EstimatedTimeStart),
                    TemplateId = model.TemplateId,
                    CageId = model.CageId,
                    Name = model.Name,
                    CleaningFrequency = model.CleaningFrequency,
                    Quantity = model.Quantity,
                    FarmId = cage.FarmId,
                    Status = FarmingBatchStatusEnum.Planning,
                    EstimatedTimeStart = model.EstimatedTimeStart,
                    EndDate = estimatedTimeEnd,
                    StartDate = model.EstimatedTimeStart // StartDate sẽ được cập nhật sau khi chuyển trạng thái
                };

                await _unitOfWork.FarmingBatches.CreateAsync(farmingBatch);
                await _unitOfWork.CommitAsync();

                var growthStages = animalTemplate.GrowthStageTemplates
                    .Select(template => new GrowthStage
                    {
                        FarmingBatchId = farmingBatch.Id,
                        Name = template.StageName,
                        WeightAnimal = template.WeightAnimal,
                        WeightAnimalExpect = template.WeightAnimal,
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
        public async Task<bool> CreateFarmingBatchMultiCageAsync(CreateFarmingBatchMultiCageModel model)
        {
            var farmConfig = await _unitOfWork.FarmConfigs.FindAll().FirstOrDefaultAsync();

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
            var ageEndMax = animalTemplate.GrowthStageTemplates.Max(gst => gst.AgeEnd);
            var estimatedTimeEnd = model.EstimatedTimeStart.Value.AddDays(ageEndMax ?? 0);
            
            // Kiểm tra số lần tạo vụ nuôi trong chuồng cho từng chuồng
            foreach (var cageItem in model.FarmingBatchItems)
            {
                var batchCount = await _unitOfWork.FarmingBatches
                    .FindByCondition(fb => fb.CageId == cageItem.CageId &&
                                            (fb.Status == FarmingBatchStatusEnum.Planning ||
                                             fb.Status == FarmingBatchStatusEnum.Active))
                    .CountAsync();
                if (batchCount >= farmConfig.MaxFarmingBatchesPerCage)
                {
                    throw new InvalidOperationException($"Chuồng {cageItem.CageId} đã đạt số lượng vụ nuôi tối đa ({farmConfig.MaxFarmingBatchesPerCage}).");
                }


               

                var existingBatch = await _unitOfWork.FarmingBatches
                    .FindByCondition(fb => fb.CageId == cageItem.CageId &&
                                            fb.Status != FarmingBatchStatusEnum.Completed &&
                                            fb.EstimatedTimeStart.HasValue &&
                                            fb.EndDate.HasValue &&
                                            (
                                                // Vụ nuôi mới bắt đầu trước và kết thúc sau vụ nuôi cũ
                                                (fb.EstimatedTimeStart.Value.Date <= model.EstimatedTimeStart.Value.Date && fb.EndDate.Value.Date >= estimatedTimeEnd.Date) ||

                                                // Vụ nuôi mới bắt đầu trong khoảng thời gian vụ nuôi cũ
                                                (fb.EstimatedTimeStart.Value.Date <= model.EstimatedTimeStart.Value.Date && fb.EndDate.Value.Date >= model.EstimatedTimeStart.Value.Date) ||

                                                // Vụ nuôi mới kết thúc trong khoảng thời gian vụ nuôi cũ
                                                (fb.EstimatedTimeStart.Value.Date <= estimatedTimeEnd.Date && fb.EndDate.Value.Date >= estimatedTimeEnd.Date)
                                            ))
                    .FirstOrDefaultAsync();

                if (existingBatch != null)
                {
                    throw new InvalidOperationException($"Đã có vụ nuôi khác trong khoảng thời gian đã chọn cho chuồng {cageItem.CageId}.");
                }
            }

            // Bắt đầu giao dịch
            await _unitOfWork.BeginTransactionAsync();

            try
            {
               
                // Lặp qua tất cả các chuồng để tạo vụ nuôi cho từng chuồng
                foreach (var cageItem in model.FarmingBatchItems)
                {
                    var cage = await _unitOfWork.Cages.FindAsync(x => x.Id == cageItem.CageId && !x.IsDeleted);
                    if (cage == null)
                    {
                        throw new ArgumentException($"Cage with ID {cageItem.CageId} does not exist or is inactive.");
                    }

                    var farmingBatch = new FarmingBatch
                    {
                        FarmingBatchCode = GenerateFarmingBatchCode(model.TemplateId, (DateTime)model.EstimatedTimeStart),
                        TemplateId = model.TemplateId,
                        CageId = cageItem.CageId,
                        Name = model.Name,
                        CleaningFrequency = model.CleaningFrequency,
                        Quantity = cageItem.Quantity,  // Sử dụng số lượng riêng cho mỗi chuồng
                        FarmId = cage.FarmId,
                        Status = FarmingBatchStatusEnum.Planning,
                        EstimatedTimeStart = model.EstimatedTimeStart,
                        EndDate = estimatedTimeEnd,
                        StartDate = model.EstimatedTimeStart // StartDate sẽ được cập nhật sau khi chuyển trạng thái
                    };

                    await _unitOfWork.FarmingBatches.CreateAsync(farmingBatch);
                    await _unitOfWork.CommitAsync();

                    // Tạo các GrowthStage cho từng chuồng
                    var growthStages = animalTemplate.GrowthStageTemplates
                        .Select(template => new GrowthStage
                        {
                            FarmingBatchId = farmingBatch.Id,
                            Name = template.StageName,
                            WeightAnimal = template.WeightAnimal,
                            WeightAnimalExpect = template.WeightAnimal,
                            AgeStart = template.AgeStart,
                            AgeEnd = template.AgeEnd,
                            FoodType = template.FoodTemplates.FirstOrDefault()?.FoodType,
                            Status = GrowthStageStatusEnum.Planning,
                            Quantity = cageItem.Quantity,  // Sử dụng số lượng chuồng của từng chuồng
                            AgeStartDate = null,
                            AgeEndDate = null,
                            SaleTypeId = template.SaleTypeId,
                            RecommendedWeightPerSession = farmingBatch.Quantity * (template.WeightAnimal ?? 0) * (template.FoodTemplates.Sum(f => f.WeightBasedOnBodyMass) ?? 0),
                            WeightBasedOnBodyMass = template.FoodTemplates.Sum(f => f.WeightBasedOnBodyMass)
                        }).ToList();

                    await _unitOfWork.GrowthStages.CreateListAsync(growthStages);
                    await _unitOfWork.CommitAsync();

                    // Tạo các TaskDaily cho từng GrowthStage
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
                                StartAt = null,
                                EndAt = null
                            })))
                        .ToList();

                    await _unitOfWork.TaskDailies.CreateListAsync(taskDailyList);

                    // Tạo lịch tiêm phòng cho từng chuồng
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
                                Session = vaccineTemplate.Session,
                                Status = VaccineScheduleStatusEnum.Upcoming,
                                Date = null
                            });
                        }
                    }

                    await _unitOfWork.VaccineSchedules.CreateListAsync(vaccineSchedules);
                }

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                Console.WriteLine($"Error in CreateFarmingBatchMultiCageAsync: {ex.Message}");
                throw new Exception("Failed to create Farming Batches. Details: " + ex.Message);
            }
        }



        public string GenerateFarmingBatchCode(Guid templateId, DateTime estimatedTime)
        {
            // Lấy phần đầu của TemplateId (hoặc bạn có thể lấy tên template)
            string templatePart = $"Template{templateId.ToString().Substring(0, 3)}"; // Lấy 3 ký tự đầu của TemplateId

            // Lấy ngày dự kiến bắt đầu, định dạng: yyyyMMdd
            string datePart = estimatedTime.ToString("yyyyMMdd");

            // Sinh mã random (kết hợp chữ và số)
            string randomPart = GenerateRandomString(6); // Sinh chuỗi random 6 ký tự (hoặc độ dài bạn muốn)

            // Ghép các phần lại để tạo FarmingBatchCode
            return $"FM-{templatePart}-{datePart}-{randomPart}";
        }

        // Phương thức sinh chuỗi ngẫu nhiên gồm 6 ký tự (chữ và số)
        private string GenerateRandomString(int length)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"; // Các ký tự hợp lệ
            Random random = new Random();
            char[] randomChars = new char[length];

            for (int i = 0; i < length; i++)
            {
                randomChars[i] = validChars[random.Next(validChars.Length)];
            }

            return new string(randomChars);
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
                var ageEndLastGrowthStage = farmingBatch.GrowthStages.Max(gs => gs.AgeEnd);
                farmingBatch.EndDate = currentStartDate.Value.AddDays(ageEndLastGrowthStage ?? 0);
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
                    FarmingbatchCode=fb.FarmingBatchCode,
                    Name = fb.Name,
                    StartDate = fb.StartDate,
                    CompleteAt = fb.CompleteAt,
                    Status = fb.Status,
                    EndDate=fb.EndDate,
                    EstimatedTimeStart = fb.EstimatedTimeStart,
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
                AffectedQuantity = farmingBatch?.AffectedQuantity,
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
                .Include(fb => fb.Cage)
                .Include(fb => fb.AnimalSales)
                    .ThenInclude(a => a.SaleType)
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
                CageName = farmingBatch.Cage.Name,
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

        public async System.Threading.Tasks.Task RunUpdateFarmingBatchesStatusAsync()
        {
            var today =DateTimeUtils.GetServerTimeInVietnamTime().Date;
            var farmingBatchesToUpdate = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.Status == FarmingBatchStatusEnum.Planning && fb.StartDate.HasValue && fb.StartDate.Value.Date == today)
                .ToListAsync();

            // Duyệt qua từng vụ nuôi và cập nhật trạng thái của chúng
            foreach (var farmingBatch in farmingBatchesToUpdate)
            {
                try
                {
                    // Cập nhật trạng thái từ Planning sang Active
                    await UpdateFarmingBatchStatusAsync(farmingBatch.Id, FarmingBatchStatusEnum.Active);
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi nếu có, có thể log lỗi nếu cần
                    Console.WriteLine($"Error updating FarmingBatch {farmingBatch.Id}: {ex.Message}");
                }
            }
        }

        public async Task<DetailedFarmingBatchReportResponse> GetDetailedFarmingBatchReportAsync(Guid farmingBatchId)
        {
            var farmingBatch = await _unitOfWork.FarmingBatches
                .FindAll()
                .Where(fb => fb.Id == farmingBatchId && fb.Status == FarmingBatchStatusEnum.Completed)
                .Include(fb => fb.Cage)
                .Include(fb => fb.AnimalSales)
                    .ThenInclude(a => a.SaleType)
                .Include(fb => fb.GrowthStages)
                    .ThenInclude(gs => gs.DailyFoodUsageLogs)
                .Include(fb => fb.GrowthStages)
                    .ThenInclude(gs => gs.VaccineSchedules)
                    .ThenInclude(vs => vs.Vaccine)
                .Include(fb => fb.MedicalSymptoms)
                    .ThenInclude(ms => ms.Prescriptions)
                .Include(fb => fb.MedicalSymptoms)
                    .ThenInclude(ms => ms.MedicalSymptomDetails)
                    .ThenInclude(msd => msd.Symptom)
                .Include(fb => fb.MedicalSymptoms)
                    .ThenInclude(ms => ms.Disease) // ✅ Thêm thông tin bệnh (Disease)
                .Include(fb => fb.GrowthStages)
                    .ThenInclude(gs => gs.EggHarvests)
                .FirstOrDefaultAsync();

            if (farmingBatch == null)
                return null;

            // Tổng doanh thu bán trứng và thịt
            var totalEggSales = farmingBatch.AnimalSales
                .Where(sale => sale.SaleType.StageTypeName == "EggSale")
                .Sum(sale => sale.UnitPrice * sale.Quantity) ?? 0;

            var totalMeatSales = farmingBatch.AnimalSales
                .Where(sale => sale.SaleType.StageTypeName == "MeatSale")
                .Sum(sale => sale.UnitPrice * sale.Quantity) ?? 0;

            // Tổng chi phí thức ăn
            var totalFoodCost = farmingBatch.GrowthStages
                .SelectMany(gs => gs.DailyFoodUsageLogs)
                .Sum(log => (decimal)log.UnitPrice * (log.ActualWeight ?? 0));

            // Tổng chi phí vaccine
            var totalVaccineCost = farmingBatch.GrowthStages
                .SelectMany(gs => gs.VaccineSchedules)
                .Sum(vaccine => vaccine.Quantity * (vaccine.ToltalPrice ?? 0));

            // Tổng chi phí thuốc
            var totalMedicineCost = farmingBatch.MedicalSymptoms
                .SelectMany(ms => ms.Prescriptions)
                .Sum(p => p.Price ?? 0);

            // Tổng số trứng thu hoạch
            var totalEggsCollected = farmingBatch.GrowthStages
                .SelectMany(gs => gs.EggHarvests)
                .Sum(eh => eh.EggCount);

            // Chi tiết vaccine tiêm trong từng giai đoạn
            var vaccineDetails = farmingBatch.GrowthStages
                .SelectMany(gs => gs.VaccineSchedules)
                .Select(vs => new VaccineDetail
                {
                    VaccineName = vs.Vaccine.Name,
                    Quantity = vs.Quantity,
                    TotalPrice = vs.ToltalPrice ?? 0,
                    DateAdministered = vs.Date
                })
                .ToList();

            // Chi tiết đơn thuốc trong quá trình nuôi
            var prescriptionDetails = farmingBatch.MedicalSymptoms
                .Select(ms => new PrescriptionDetail
                {
                    PrescriptionId = ms.Prescriptions.FirstOrDefault()?.Id ?? Guid.Empty,
                    Diagnosis = ms.Diagnosis,
                    AffectedQuantity = ms.AffectedQuantity ?? 0,
                    PrescriptionPrice = ms.Prescriptions.Sum(p => p.Price ?? 0),
                    DiseaseName = ms.Disease != null ? ms.Disease.Name : "Unknown", // ✅ Thêm thông tin bệnh (Disease)
                    DiseaseDescription = ms.Disease != null ? ms.Disease.Description : "N/A",
                    Symptoms = ms.MedicalSymptomDetails.Select(msd => msd.Symptom.SymptomName).ToList()
                })
                .ToList();

            // Chi tiết loại thức ăn và tổng số ký đã sử dụng
            var foodUsageDetails = farmingBatch.GrowthStages
                .SelectMany(gs => gs.DailyFoodUsageLogs)
                .GroupBy(log => log.Stage.FoodType)
                .Select(group => new FoodUsageDetail
                {
                    FoodType = group.Key,
                    TotalWeightUsed = group.Sum(log => log.ActualWeight ?? 0)
                })
                .ToList();

            // Lợi nhuận ròng
            var netProfit = ((decimal)totalEggSales + (decimal)totalMeatSales) - (totalFoodCost + (decimal)totalVaccineCost + totalMedicineCost);

            return new DetailedFarmingBatchReportResponse
            {
                FarmingBatchId = farmingBatch.Id,
                FarmingBatchName = farmingBatch.Name,
                CageName = farmingBatch.Cage.Name,
                StartDate = farmingBatch.StartDate,
                EndDate = farmingBatch.CompleteAt,
                TotalEggSales = (decimal)totalEggSales,
                TotalMeatSales = (decimal)totalMeatSales,
                TotalFoodCost = totalFoodCost,
                TotalVaccineCost = (decimal)totalVaccineCost,
                TotalMedicineCost = totalMedicineCost,
                TotalEggsCollected = totalEggsCollected,
                NetProfit = netProfit,
                VaccineDetails = vaccineDetails,
                PrescriptionDetails = prescriptionDetails,
                FoodUsageDetails = foodUsageDetails
            };
        }


    }
}
