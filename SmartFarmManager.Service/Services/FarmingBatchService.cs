﻿using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Cages;
using SmartFarmManager.Service.BusinessModels.FarmingBatch;
using SmartFarmManager.Service.BusinessModels.Task;
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
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingActiveBatch = await _unitOfWork.FarmingBatches
                    .FindByCondition(fb => fb.CageId == model.CageId && fb.Status == FarmingBatchStatusEnum.Active)
                    .AnyAsync();

                if (existingActiveBatch)
                {
                    throw new InvalidOperationException($"Cage {model.CageId} already has an active farming batch.");
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
                    Species = model.Species,
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
                        Status = GrowthStageStatusEnum.Planning,
                        AgeStartDate = null, // Sẽ được cập nhật khi trạng thái chuyển sang Active
                        AgeEndDate = null,
                        SaleTypeId= template.SaleTypeId,
                        RecommendedWeightPerSession = template.FoodTemplates.Sum(f => f.RecommendedWeightPerSession),
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
                .FirstOrDefaultAsync();

            if (farmingBatch == null)
            {
                throw new ArgumentException($"FarmingBatch with ID {farmingBatchId} does not exist.");
            }

            // Kiểm tra trạng thái hiện tại
            if (farmingBatch.Status == newStatus)
            {
                throw new InvalidOperationException($"FarmingBatch is already in '{newStatus}' status.");
            }

            if (newStatus != FarmingBatchStatusEnum.Active && newStatus != FarmingBatchStatusEnum.Completed)
            {
                throw new ArgumentException($"Invalid status transition to '{newStatus}'.");
            }

            if (farmingBatch.Status == FarmingBatchStatusEnum.Planning && newStatus == FarmingBatchStatusEnum.Active)
            {
                // Chuyển trạng thái sang Active
                farmingBatch.Status = FarmingBatchStatusEnum.Active;
                farmingBatch.StartDate = DateTimeUtils.VietnamNow();

                var currentStartDate = farmingBatch.StartDate;  

                // Cập nhật GrowthStages
                foreach (var stage in farmingBatch.GrowthStages.OrderBy(gs => gs.AgeStart))
                {
                    stage.AgeStartDate = currentStartDate;
                    stage.AgeEndDate = currentStartDate.Value.AddDays(stage.AgeEnd - stage.AgeStart ?? 0);

                    stage.Status = stage.AgeStartDate == farmingBatch.StartDate
                        ? GrowthStageStatusEnum.Active
                        : GrowthStageStatusEnum.Upcoming;

                    currentStartDate = stage.AgeEndDate.Value.AddDays(1);

                    // Cập nhật TaskDaily
                    foreach (var taskDaily in stage.TaskDailies)
                    {
                        taskDaily.StartAt = stage.AgeStartDate;
                        taskDaily.EndAt = stage.AgeEndDate;
                        await _unitOfWork.TaskDailies.UpdateAsync(taskDaily);
                    }

                    // Cập nhật VaccineSchedule
                    foreach (var vaccineSchedule in stage.VaccineSchedules)
                    {
                        if (stage.AgeStartDate.HasValue)
                        {
                            vaccineSchedule.Date = stage.AgeStartDate.Value.AddDays(
                                (vaccineSchedule.ApplicationAge ?? 0) - (stage.AgeStart ?? 0)
                            );

                            if (vaccineSchedule.Date > DateTimeUtils.VietnamNow().Date)
                            {
                                vaccineSchedule.Status = VaccineScheduleStatusEnum.Upcoming;
                            }
                            else if (vaccineSchedule.Date == DateTimeUtils.VietnamNow().Date)
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
                // Chuyển trạng thái sang Completed
                farmingBatch.Status = FarmingBatchStatusEnum.Completed;
                farmingBatch.CompleteAt = DateTimeUtils.VietnamNow();

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

            return true;
        }

        public async Task<PagedResult<FarmingBatchModel>> GetFarmingBatchesAsync(string? status, string? cageName, string? name, string? species, DateTime? startDateFrom, DateTime? startDateTo, int pageNumber, int pageSize, Guid? cageId)
        {
            var query = _unitOfWork.FarmingBatches.FindAll()
                .Include(fb => fb.Cage) // Include related Cage
                .Include(fb => fb.Template)
                .AsQueryable();

            // Apply Filters
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(x => x.Status == status);
            }

            if (!string.IsNullOrEmpty(cageName))
            {
                query = query.Where(x => x.Cage.Name.Contains(cageName));
            }

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(x => x.Name.Contains(name));
            }
            if (!string.IsNullOrEmpty(species))
            {
                query = query.Where(x => x.Species.Contains(species));
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
                    Species = fb.Species,
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
                        AnimalType = fb.Cage.AnimalType,
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
            var currentDate = DateOnly.FromDateTime(DateTimeUtils.VietnamNow());

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
                Species = farmingBatch.Species,
                StartDate = farmingBatch.StartDate,
                CompleteAt = farmingBatch.CompleteAt,
                Status = farmingBatch.Status,
                CleaningFrequency = farmingBatch.CleaningFrequency,
                Quantity = farmingBatch.Quantity,
            };
        }


    }
}
