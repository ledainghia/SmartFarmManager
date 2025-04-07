﻿using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.BusinessModels.DailyFoodUsageLog;
using SmartFarmManager.Service.BusinessModels.FarmingBatch;
using SmartFarmManager.Service.BusinessModels.GrowthStage;
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
        private readonly NotificationService _notificationService;
        private readonly INotificationService _notificationUserService;
        private readonly EmailService _emailService;

        public FarmingBatchService(IUnitOfWork unitOfWork, ITaskService taskService, EmailService emailService,NotificationService notificationService, INotificationService notificationUserService)
        {
            _unitOfWork = unitOfWork;
            _taskService = taskService;
            _emailService = emailService;
            _notificationService = notificationService;
            _notificationUserService = notificationUserService;
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


        public async Task<PagedResult<FarmingBatchModel>> GetFarmingBatchesAsync(string? keySearch,Guid? farmId, string? cageName, string? name, string? species, DateTime? startDateFrom, DateTime? startDateTo, int pageNumber, int pageSize, Guid? cageId, bool? isCancel)
        {
            var query = _unitOfWork.FarmingBatches.FindAll()
                .Include(fb => fb.Cage) // Include related Cage
                .Include(fb => fb.Template)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keySearch))
            {
                query = query.Where(fb =>
                    fb.Name.Contains(keySearch) ||                    
                    fb.Status.Contains(keySearch)
                    || fb.Cage.Name.Contains(keySearch));
            }

            if (!farmId.HasValue)
            {
                query = query.Where(x => x.FarmId == farmId);
            }
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
                DeadQuantity = farmingBatch?.DeadQuantity
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
                DeadQuantity = fb.DeadQuantity,
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

            var growthStageReports = farmingBatch.GrowthStages.Select(gs => new GrowthStageReportModel
            {
                StageId = gs.Id,
                StageName = gs.Name,
                AgeStartDate = gs.AgeStartDate,
                AgeEndDate = gs.AgeEndDate,
                WeightAnimal = gs.WeightAnimal,
                WeightAnimalExpect = gs.WeightAnimalExpect,
                Quantity = gs.Quantity,
                DeadQuantity = gs.DeadQuantity,

                Vaccines = gs.VaccineSchedules.Select(vs => new VaccineDetail
                {
                    VaccineName = vs.Vaccine.Name,
                    Quantity = vs.Quantity,
                    TotalPrice = vs.ToltalPrice ?? 0,
                    DateAdministered = vs.Date
                }).ToList(),

                Foods = gs.DailyFoodUsageLogs.GroupBy(f => f.Stage.FoodType).Select(group => new FoodUsageDetail
                {
                    FoodType = group.Key,
                    TotalWeightUsed = group.Sum(log => log.ActualWeight ?? 0)
                }).ToList(),

                Prescriptions = farmingBatch.MedicalSymptoms
            .Where(ms => ms.CreateAt >= gs.AgeStartDate && ms.CreateAt <= gs.AgeEndDate)
            .Select(ms => new PrescriptionDetail
            {
                PrescriptionId = ms.Prescriptions.FirstOrDefault()?.Id ?? Guid.Empty,
                Diagnosis = ms.Diagnosis,
                AffectedQuantity = ms.AffectedQuantity ?? 0,
                PrescriptionPrice = ms.Prescriptions.Sum(p => p.Price ?? 0),
                DiseaseName = ms.Disease?.Name ?? "Unknown",
                DiseaseDescription = ms.Disease?.Description ?? "N/A",
                Symptoms = ms.MedicalSymptomDetails.Select(d => d.Symptom.SymptomName).ToList()
            }).ToList()
            }).ToList();

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
                QuantityAnimal = farmingBatch.Quantity,
                DeadQuantity = farmingBatch.DeadQuantity,
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
                FoodUsageDetails = foodUsageDetails,
                GrowthStageReports = growthStageReports
            };
        }
        public async System.Threading.Tasks.Task CheckAndNotifyAdminForUpcomingFarmingBatchesAsync()
        {

            var today = DateTimeUtils.GetServerTimeInVietnamTime().Date;
            var twoDaysLater = today.AddDays(2);

            var farmingBatches = await _unitOfWork.FarmingBatches           
                .FindByCondition(fb=>fb.StartDate.HasValue &&
                                     fb.StartDate.Value.Date >=today.Date &&
                                     fb.StartDate.Value.Date <= twoDaysLater.Date &&
                                     fb.Status == FarmingBatchStatusEnum.Planning)
                .Include(fb=>fb.Cage)
                .ToListAsync();

            foreach(var farmingBatch in farmingBatches)
            {
                var activeBatchExists = await _unitOfWork.FarmingBatches
                    .FindByCondition(fb => fb.CageId == farmingBatch.CageId && fb.Status == FarmingBatchStatusEnum.Active)
                    .AnyAsync();

                if (activeBatchExists)
                {
                    var admin = await _unitOfWork.Users
               .FindByCondition(u => u.Role.RoleName == "Admin Farm")
               .Include(u => u.Role)
               .FirstOrDefaultAsync();

                    var notiType = await _unitOfWork.NotificationsTypes
                        .FindByCondition(nt => nt.NotiTypeName == "FarmingBatchSchedule")
                        .FirstOrDefaultAsync();

                    var notification = new DataAccessObject.Models.Notification
                    {
                        UserId = admin.Id,
                        NotiTypeId = (Guid)notiType?.Id,
                        Content = $"Sắp tới có vụ nuôi {farmingBatch.Name} trong chuồng {farmingBatch.Cage.Name} bắt đầu vào ngày {farmingBatch.EstimatedTimeStart.Value.ToString("yyyy-MM-dd")}. Vui lòng xem xét và điều chỉnh lịch cho vụ nuôi nếu có xung đột.",
                        CreatedAt = DateTime.UtcNow,
                        MedicalSymptomId = null,  
                        IsRead = false
                    };
                    await _notificationUserService.CreateNotificationAsync(notification);
                    await _notificationService.SendNotification(admin.DeviceId, "Thông báo về vụ nuôi sắp tới", notification);

                    // Gửi email nhắc nhở admin
                    await _emailService.SendReminderEmailAsync(admin.Email, admin.FullName, "Cảnh báo từ hệ thống",
                        $"Sắp tới có vụ nuôi {farmingBatch.Name} trong chuồng {farmingBatch.CageId} bắt đầu vào ngày {farmingBatch.EstimatedTimeStart.Value.ToString("yyyy-MM-dd")}. Vui lòng kiểm tra và điều chỉnh lịch vụ nuôi nếu có trùng lặp.");
                }
                else
                {
                    var admin = await _unitOfWork.Users
               .FindByCondition(u => u.Role.RoleName == "Admin Farm")
               .Include(u => u.Role)
               .FirstOrDefaultAsync();

                    var notiType = await _unitOfWork.NotificationsTypes
                        .FindByCondition(nt => nt.NotiTypeName == "FarmingBatchSchedule")
                        .FirstOrDefaultAsync();

                    var notification = new DataAccessObject.Models.Notification
                    {
                        UserId = admin.Id,
                        NotiTypeId = (Guid)notiType?.Id,
                        Content = $"Sắp tới có vụ nuôi {farmingBatch.Name} trong chuồng {farmingBatch.Cage.Name} bắt đầu vào ngày {farmingBatch.EstimatedTimeStart.Value.ToString("yyyy-MM-dd")}.",
                        CreatedAt = DateTime.UtcNow,
                        MedicalSymptomId = null,  // Không phải là triệu chứng y tế
                        IsRead = false
                    };

                    // Gửi thông báo cho admin
                    await _notificationUserService.CreateNotificationAsync(notification);
                    await _notificationService.SendNotification(admin.DeviceId, "Thông báo về vụ nuôi sắp tới", notification);

                    // Gửi email nhắc nhở admin
                    await _emailService.SendReminderEmailAsync(admin.Email, admin.FullName, "Cảnh báo từ hệ thống",
                        $"Sắp tới có vụ nuôi {farmingBatch.Name} trong chuồng {farmingBatch.CageId} bắt đầu vào ngày {farmingBatch.EstimatedTimeStart.Value.ToString("yyyy-MM-dd")}. Vui lòng kiểm tra và đảm bảo không có xung đột lịch.");
                }
            }

        }
        public async Task<CageFarmingStageModel> GetCurrentFarmingStageWithCageAsync(Guid cageId)
        {
            // 🔹 Lấy thông tin chuồng
            var cage = await _unitOfWork.Cages
                .FindByCondition(c => c.Id == cageId)
                .Include(c => c.CageStaffs)
                .ThenInclude(cs => cs.StaffFarm)
                .FirstOrDefaultAsync();

            if (cage == null)
                return null;

            // 🔹 Lấy vụ nuôi đang hoạt động (`Active`) trong chuồng
            var farmingBatch = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.CageId == cageId && fb.Status == FarmingBatchStatusEnum.Active)
                .Include(fb => fb.GrowthStages) // Lấy danh sách các giai đoạn phát triển
                .Include(gs => gs.Template)
                .FirstOrDefaultAsync();

            // Nếu không có vụ nuôi active thì chỉ trả về thông tin Cage
            if (farmingBatch == null)
            {
                return new CageFarmingStageModel
                {
                    Id = cage.Id,
                    PenCode = cage.PenCode,
                    FarmId = cage.FarmId,
                    Name = cage.Name,
                    Area = cage.Area,
                    Location = cage.Location,
                    Capacity = cage.Capacity,
                    BoardCode = cage.BoardCode,
                    BoardStatus = cage.BoardStatus,
                    CreatedDate = cage.CreatedDate,
                    ModifiedDate = cage.ModifiedDate,
                    CameraUrl = cage.CameraUrl,
                    StaffId = cage.CageStaffs.FirstOrDefault()?.StaffFarmId ?? Guid.Empty,
                    StaffName = cage.CageStaffs.FirstOrDefault()?.StaffFarm.FullName ?? "Unknown",
                    FarmingBatchStageModel = null
                };
            }

            // 🔹 Tìm giai đoạn phát triển hiện tại dựa trên thời gian
            var today = DateTimeUtils.GetServerTimeInVietnamTime().Date;
            var currentGrowthStage = farmingBatch.GrowthStages
                .Where(gs => gs.AgeStartDate.HasValue && gs.AgeEndDate.HasValue)
                .FirstOrDefault(gs => gs.AgeStartDate.Value.Date <= today && gs.AgeEndDate.Value.Date >= today);

            return new CageFarmingStageModel
            {
                Id = cage.Id,
                PenCode = cage.PenCode,
                FarmId = cage.FarmId,
                Name = cage.Name,
                Area = cage.Area,
                Location = cage.Location,
                Capacity = cage.Capacity,
                BoardCode = cage.BoardCode,
                BoardStatus = cage.BoardStatus,
                CreatedDate = cage.CreatedDate,
                ModifiedDate = cage.ModifiedDate,
                CameraUrl = cage.CameraUrl,
                StaffId = cage.CageStaffs.FirstOrDefault()?.StaffFarmId ?? Guid.Empty,
                StaffName = cage.CageStaffs.FirstOrDefault()?.StaffFarm.FullName ?? "Unknown",
                FarmingBatchStageModel = new FarmingBatchStageModel
                {
                    Id = farmingBatch.Id,
                    FarmingbatchCode = farmingBatch.Name,
                    Name = farmingBatch.Name,
                    Species = farmingBatch.Template.Species,
                    StartDate = farmingBatch.StartDate,
                    CompleteAt = farmingBatch.CompleteAt,
                    EstimatedTimeStart = farmingBatch.StartDate,
                    EndDate = farmingBatch.EndDate,
                    Status = farmingBatch.Status,
                    CleaningFrequency = farmingBatch.CleaningFrequency,
                    Quantity = farmingBatch.Quantity,
                    DeadQuantity = farmingBatch.DeadQuantity,
                    GrowthStageDetails = currentGrowthStage == null ? null : new GrowthStageDetailModel
                    {
                        Id = currentGrowthStage.Id,
                        FarmingBatchId = farmingBatch.Id,
                        Name = currentGrowthStage.Name,
                        WeightAnimal = currentGrowthStage.WeightAnimal,
                        Quantity = currentGrowthStage.Quantity,
                        AgeStart = currentGrowthStage.AgeStart,
                        AgeEnd = currentGrowthStage.AgeEnd,
                        FoodType = currentGrowthStage.FoodType,
                        AgeStartDate = currentGrowthStage.AgeStartDate,
                        AgeEndDate = currentGrowthStage.AgeEndDate,
                        Status = currentGrowthStage.Status,
                        AffectQuantity = currentGrowthStage.AffectedQuantity,
                        DeadQuantity = currentGrowthStage.DeadQuantity,
                        RecommendedWeightPerSession = currentGrowthStage.RecommendedWeightPerSession,
                        WeightBasedOnBodyMass = currentGrowthStage.WeightBasedOnBodyMass,
                    }
                }
            };
        }

        public async Task<bool> UpdateStartDateAsync(Guid farmingBatchId, DateTime newStartDate)
        {
            var farmingBatch = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.Id == farmingBatchId)
                .Include(fb => fb.GrowthStages)
                .FirstOrDefaultAsync();
            if (farmingBatch == null)
            {
                throw new ArgumentException("FarmingBatch không tồn tại.");
            }
            if (farmingBatch.Status != FarmingBatchStatusEnum.Planning)
            {
                throw new InvalidOperationException("Chỉ có vụ nuôi đang ở trạng thái 'Planning' mới có thể thay đổi ngày bắt đầu.");
            }
            if (newStartDate <= farmingBatch.EstimatedTimeStart)
            {
                throw new InvalidOperationException("Ngày bắt đầu mới phải lớn hơn ngày dự kiến bắt đầu.");
            }
            var conflictingBatch = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.CageId == farmingBatch.CageId &&
                                        fb.Status == FarmingBatchStatusEnum.Active && 
                                        fb.EstimatedTimeStart.HasValue &&
                                        fb.EstimatedTimeStart.Value.Date == newStartDate.Date) 
                .FirstOrDefaultAsync();

            if (conflictingBatch != null)
            {
                throw new InvalidOperationException($"Chuồng này đã có vụ nuôi đang hoạt động vào ngày {newStartDate.ToString("yyyy-MM-dd")}. Cần điều chỉnh ngày bắt đầu.");
            }
            farmingBatch.StartDate = newStartDate;  // Cập nhật EstimatedTimeStart (ngày dự kiến)

            var ageEndMax = farmingBatch.GrowthStages.Max(gs => gs.AgeEnd);
            farmingBatch.EndDate = newStartDate.AddDays(ageEndMax ?? 0);

            await _unitOfWork.FarmingBatches.UpdateAsync(farmingBatch);
            await _unitOfWork.CommitAsync();

            // Trả về true nếu cập nhật thành công
            return true;
        }

        public async Task<FarmingBatchDetailModel> GetFarmingBatchDetailAsync(Guid farmingBatchId)
        {
            var farmingBatch = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.Id == farmingBatchId)
                .Include(fb => fb.Cage)
                .Include(fb=>fb.Template)
                .Include(fb => fb.AnimalSales)  
                .Include(fb => fb.GrowthStages) 
                .Include(fb=>fb.MedicalSymptoms)
                .FirstOrDefaultAsync();

            if (farmingBatch == null)
            {
                throw new KeyNotFoundException($"Farming batch with ID {farmingBatchId} not found.");
            }

            var detailModel = new FarmingBatchDetailModel
            {
                Id = farmingBatch.Id,
                Name = farmingBatch.Name,
                Status = farmingBatch.Status,
                StartDate = farmingBatch.StartDate,
                EndDate = farmingBatch.EndDate,
                CageId = farmingBatch.CageId,
                CageName = farmingBatch.Cage.Name,
                CleaningFrequency = farmingBatch.CleaningFrequency,
                CompleteAt = farmingBatch.CompleteAt,
                DeadQuantity = farmingBatch.DeadQuantity,
                EstimatedTimeStart=farmingBatch.EstimatedTimeStart,
                FarmId = farmingBatch.FarmId,
                FarmingBatchCode = farmingBatch.FarmingBatchCode,
                Quantity = farmingBatch.Quantity,
                TemplateName = farmingBatch.Template.Name,
                AnimalSales = farmingBatch.AnimalSales.Select(asale => new AnimalSaleDetaiInFarmingBatchlModel
                {
                    Id = asale.Id,
                    SaleDate = (DateTime) asale.SaleDate,
                    Quantity = asale.Quantity,
                    Total = asale.Total,
                    UnitPrice = (double)asale.UnitPrice
                }).ToList(),
                GrowthStages = farmingBatch.GrowthStages.Select(gs => new GrowthStageDetailInFarmingBactchModel
                {
                    Id = gs.Id,
                    Name = gs.Name,                 
                    AgeStartDate = gs.AgeStartDate,
                    AgeEndDate = gs.AgeEndDate
                }).ToList(),
                MedicalSymptoms=farmingBatch.MedicalSymptoms.Select(ms => new MedicalSymptomInFarmingBatchModel
                {
                    Id = ms.Id,
                    Diagnosis = ms.Diagnosis,
                    AffectedQuantity = ms.AffectedQuantity,
                    IsEmergency = ms.IsEmergency,
                    Notes=ms.Notes,
                    QuantityInCage = ms.QuantityInCage,
                    Status= ms.Status

                }).ToList()


            };

            return detailModel;
        }

    }
}
