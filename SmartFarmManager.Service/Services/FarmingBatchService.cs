﻿using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.FarmingBatch;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Shared;

namespace SmartFarmManager.Service.Services
{
    public class FarmingBatchService : IFarmingBatchService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FarmingBatchService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateFarmingBatchAsync(CreateFarmingBatchModel model)
        {
            // Bắt đầu transaction
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // 1. Kiểm tra CageId có FarmingBatch nào đang hoạt động không
                var existingActiveBatch = await _unitOfWork.FarmingBatches
                    .FindByCondition(fb => fb.CageId == model.CageId && fb.Status == FarmingBatchStatusEnum.Active)
                    .AnyAsync();

                if (existingActiveBatch)
                {
                    throw new InvalidOperationException($"Cage {model.CageId} already has an active farming batch.");
                }

                // 2. Validate TemplateId
                var animalTemplate = await _unitOfWork.AnimalTemplates
                    .FindByCondition(a => a.Id == model.TemplateId && a.Status == "Active")
                    .Include(a => a.GrowthStageTemplates)
                    .Include(a => a.VaccineTemplates)
                    .FirstOrDefaultAsync();

                if (animalTemplate == null)
                {
                    throw new ArgumentException($"Animal template with ID {model.TemplateId} does not exist or is inactive.");
                }

                // 3. Validate CageId
                var cage = await _unitOfWork.Cages.FindAsync(x => x.Id == model.CageId && !x.IsDeleted);
                if (cage == null)
                {
                    throw new ArgumentException($"Cage with ID {model.CageId} does not exist or is inactive.");
                }

                // 4. Create FarmingBatch
                var farmingBatch = new FarmingBatch
                {
                    TemplateId = model.TemplateId,
                    CageId = model.CageId,
                    Name = model.Name,
                    Species = model.Species,
                    CleaningFrequency = model.CleaningFrequency,
                    Quantity = model.Quantity,
                    FarmId = model.FarmId,
                    Status = FarmingBatchStatusEnum.Planning,
                    StartDate = null,
                };

                await _unitOfWork.FarmingBatches.CreateAsync(farmingBatch);
                await _unitOfWork.CommitAsync();

                // 5. Generate GrowthStages
                var growthStages = animalTemplate.GrowthStageTemplates.Select(template => new GrowthStage
                {
                    FarmingBatchId = farmingBatch.Id,
                    Name = template.StageName,
                    WeightAnimal = template.WeightAnimal,
                    AgeStart = template.AgeStart,
                    AgeEnd = template.AgeEnd,
                    Status = FarmingBatchStatusEnum.Planning,
                    AgeStartDate = null,
                    AgeEndDate=null,
                    RecommendedWeightPerSession = template.FoodTemplates.Sum(f => f.RecommendedWeightPerSession),
                    WeightBasedOnBodyMass = template.FoodTemplates.Sum(f => f.WeightBasedOnBodyMass)
                }).ToList();

                await _unitOfWork.GrowthStages.CreateListAsync(growthStages);
                await _unitOfWork.CommitAsync();

                var taskDailyList = growthStages.SelectMany(stage =>animalTemplate.GrowthStageTemplates
                   .Where(template =>
                    template.StageName == stage.Name &&
                    template.AgeStart == stage.AgeStart &&
                    template.AgeEnd == stage.AgeEnd)
                 .SelectMany(template => template.TaskDailyTemplates.Select(taskTemplate => new TaskDaily
                     {
                        GrowthStageId = stage.Id, // Gắn GrowthStageId từ bản ghi đã tạo
                        TaskTypeId = taskTemplate.TaskTypeId,
                        TaskName = taskTemplate.TaskName,
                        Description = taskTemplate.Description,
                        Session = taskTemplate.Session,
                        StartAt = null,
                        EndAt = null
                        }))
                 ).ToList();


                await _unitOfWork.TaskDailies.CreateListAsync(taskDailyList);

                // 7. Generate VaccineSchedules
                var vaccineSchedules = new List<VaccineSchedule>();

                foreach (var vaccineTemplate in animalTemplate.VaccineTemplates)
                {
                    // Tìm Vaccine theo tên
                    var vaccine = await _unitOfWork.Vaccines
                        .FindByCondition(v => v.Name == vaccineTemplate.VaccineName)
                        .FirstOrDefaultAsync();

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
                            VaccineId = vaccine.Id, // Gán VaccineId từ bản ghi tìm được
                            Quantity = farmingBatch.Quantity,
                            ApplicationAge = vaccineTemplate.ApplicationAge,
                            Status = FarmingBatchStatusEnum.Planning
                        });
                    }
                }


                await _unitOfWork.VaccineSchedules.CreateListAsync(vaccineSchedules);

                // 8. Commit all changes
                await _unitOfWork.CommitAsync();

                // Hoàn tất transaction
                return true;
            }
            catch (Exception)
            {
                // Rollback nếu có lỗi
                await _unitOfWork.RollbackAsync();
                throw; // Re-throw exception để controller xử lý
            }
        }

        public async Task<bool> UpdateFarmingBatchStatusAsync(Guid farmingBatchId, string newStatus)
        {
            // 1. Lấy thông tin FarmingBatch
            var farmingBatch = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.Id == farmingBatchId)
                .Include(fb => fb.GrowthStages)
                .ThenInclude(gs => gs.VaccineSchedules)
                .FirstOrDefaultAsync();

            if (farmingBatch == null)
            {
                throw new ArgumentException($"FarmingBatch with ID {farmingBatchId} does not exist.");
            }

            // 2. Kiểm tra trạng thái hiện tại
            if (farmingBatch.Status == newStatus)
            {
                throw new InvalidOperationException($"FarmingBatch is already in '{newStatus}' status.");
            }

            if (newStatus != FarmingBatchStatusEnum.Active && newStatus != FarmingBatchStatusEnum.Completed)
            {
                throw new ArgumentException($"Invalid status transition to '{newStatus}'.");
            }

            // 3. Chuyển trạng thái từ Planning -> Active
            if (farmingBatch.Status == FarmingBatchStatusEnum.Planning && newStatus == FarmingBatchStatusEnum.Active)
            {
                farmingBatch.Status = FarmingBatchStatusEnum.Active;
                farmingBatch.StartDate = DateTime.UtcNow;

                // Tính toán ngày bắt đầu và kết thúc cho GrowthStages
                var currentStartDate = farmingBatch.StartDate;
                foreach (var stage in farmingBatch.GrowthStages.OrderBy(gs => gs.AgeStart))
                {
                    stage.AgeStartDate = currentStartDate;
                    stage.AgeEndDate = currentStartDate.Value.AddDays(stage.AgeEnd - stage.AgeStart ?? 0);

                    stage.Status = stage.AgeStartDate == farmingBatch.StartDate
                        ? GrowthStageStatusEnum.Active
                        : GrowthStageStatusEnum.Upcoming;

                    currentStartDate = stage.AgeEndDate.Value.AddDays(1); // Ngày bắt đầu của giai đoạn tiếp theo
                }

                // Cập nhật ngày và trạng thái cho VaccineSchedules
                foreach (var growthStage in farmingBatch.GrowthStages)
                {
                    foreach (var vaccineSchedule in growthStage.VaccineSchedules)
                    {
                        // Tính ngày tiêm từ AgeStartDate của GrowthStage
                        if (growthStage.AgeStartDate.HasValue)
                        {
                            vaccineSchedule.Date = growthStage.AgeStartDate.Value.AddDays(
                                (vaccineSchedule.ApplicationAge ?? 0) - (growthStage.AgeStart ?? 0)
                            );

                            // Cập nhật trạng thái VaccineSchedule
                            if (vaccineSchedule.Date > DateTime.UtcNow.Date)
                            {
                                vaccineSchedule.Status = VaccineScheduleStatusEnum.Upcoming;
                            }
                            else if (vaccineSchedule.Date == DateTime.UtcNow.Date)
                            {
                                vaccineSchedule.Status = VaccineScheduleStatusEnum.Completed;
                            }
                            else
                            {
                                vaccineSchedule.Status = VaccineScheduleStatusEnum.Missed;
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"GrowthStage '{growthStage.Name}' does not have a valid AgeStartDate.");
                        }
                    }
                }
            }
            else if (newStatus == FarmingBatchStatusEnum.Completed)
            {
                // 4. Chuyển trạng thái sang Completed
                farmingBatch.Status = FarmingBatchStatusEnum.Completed;
                farmingBatch.CompleteAt = DateTime.UtcNow;

                foreach (var stage in farmingBatch.GrowthStages)
                {
                    stage.Status = GrowthStageStatusEnum.Completed;
                }

                foreach (var growthStage in farmingBatch.GrowthStages)
                {
                    foreach (var vaccineSchedule in growthStage.VaccineSchedules)
                    {
                        if (vaccineSchedule.Status == VaccineScheduleStatusEnum.Upcoming)
                        {
                            vaccineSchedule.Status = VaccineScheduleStatusEnum.Missed;
                        }
                    }
                }
            }

            // 5. Lưu thay đổi
            await _unitOfWork.FarmingBatches.UpdateAsync(farmingBatch);
            await _unitOfWork.CommitAsync();

            return true;
        }



    }
}
