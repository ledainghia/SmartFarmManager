using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.GrowthStage;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFarmManager.Service.BusinessModels.TaskDaily;
using SmartFarmManager.Service.BusinessModels.VaccineSchedule;
using SmartFarmManager.Service.Shared;

namespace SmartFarmManager.Service.Services
{
    public class GrowthStageService:IGrowthStageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GrowthStageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<GrowthStageItemModel>> GetGrowthStagesAsync(GrowthStageFilterModel filter)
        {
            // Query cơ bản
            var query = _unitOfWork.GrowthStages.FindAll(false).AsQueryable();

            // Áp dụng bộ lọc
            if (filter.FarmingBatchId.HasValue)
            {
                query = query.Where(g => g.FarmingBatchId == filter.FarmingBatchId.Value);
            }

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(g => g.Name.Contains(filter.Name));
            }

            if (filter.AgeStart.HasValue)
            {
                query = query.Where(g => g.AgeStart >= filter.AgeStart.Value);
            }

            if (filter.AgeEnd.HasValue)
            {
                query = query.Where(g => g.AgeEnd <= filter.AgeEnd.Value);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(g => g.Status == filter.Status);
            }

            // Sắp xếp theo OrderBy và Order
            query = filter.OrderBy.ToLower() switch
            {
                "name" => filter.Order.ToLower() == "desc" ? query.OrderByDescending(g => g.Name) : query.OrderBy(g => g.Name),
                "agestart" => filter.Order.ToLower() == "desc" ? query.OrderByDescending(g => g.AgeStart) : query.OrderBy(g => g.AgeStart),
                "ageend" => filter.Order.ToLower() == "desc" ? query.OrderByDescending(g => g.AgeEnd) : query.OrderBy(g => g.AgeEnd),
                "status" => filter.Order.ToLower() == "desc" ? query.OrderByDescending(g => g.Status) : query.OrderBy(g => g.Status),
                _ => query.OrderBy(g => g.AgeStart) // Mặc định là AgeStart và tăng dần
            };

            // Tổng số phần tử
            var totalItems = await query.CountAsync();

            // Phân trang và lấy dữ liệu
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(g => new GrowthStageItemModel
                {
                    Id = g.Id,
                    FarmingBatchId = g.FarmingBatchId,
                    Name = g.Name,
                    WeightAnimal = g.WeightAnimal,
                    Quantity = g.Quantity,
                    AgeStart = g.AgeStart,
                    AgeEnd = g.AgeEnd,
                    AgeStartDate = g.AgeStartDate,
                    AgeEndDate = g.AgeEndDate,
                    Status = g.Status,
                    RecommendedWeightPerSession = g.RecommendedWeightPerSession,
                    WeightBasedOnBodyMass = g.WeightBasedOnBodyMass
                })
                .ToListAsync();

            var result = new PaginatedList<GrowthStageItemModel>(items, totalItems, filter.PageNumber, filter.PageSize);

            // Kết quả phân trang
            return new PagedResult<GrowthStageItemModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage,
            };
        }

        public async Task<GrowthStageDetailModel> GetGrowthStageDetailAsync(Guid id)
        {
            var growthStage = await _unitOfWork.GrowthStages
                .FindByCondition(gs => gs.Id == id)
                .Include(gs => gs.TaskDailies)
                .Include(gs => gs.VaccineSchedules)
                .FirstOrDefaultAsync();

            if (growthStage == null)
            {
                return null;
            }

            return new GrowthStageDetailModel
            {
                Id = growthStage.Id,
                FarmingBatchId = growthStage.FarmingBatchId,
                Name = growthStage.Name,
                WeightAnimal = growthStage.WeightAnimal,
                Quantity = growthStage.Quantity,
                AgeStart = growthStage.AgeStart,
                AgeEnd = growthStage.AgeEnd,
                AgeStartDate = growthStage.AgeStartDate,
                AgeEndDate = growthStage.AgeEndDate,
                Status = growthStage.Status,
                RecommendedWeightPerSession = growthStage.RecommendedWeightPerSession,
                WeightBasedOnBodyMass = growthStage.WeightBasedOnBodyMass,
                TaskDailies = growthStage.TaskDailies.Select(td => new TaskDailyModel
                {
                    Id = td.Id,
                    GrowthStageId = td.GrowthStageId,
                    TaskTypeId = td.TaskTypeId,
                    TaskName = td.TaskName,
                    Description = td.Description,
                    Session = td.Session,
                    StartAt = td.StartAt,
                    EndAt = td.EndAt
                }).ToList(),
                VaccineSchedules = growthStage.VaccineSchedules.Select(vs => new VaccineScheduleModel
                {
                    Id = vs.Id,
                    StageId = vs.StageId,
                    VaccineId = vs.VaccineId,
                    Date = vs.Date,
                    ApplicationAge = vs.ApplicationAge,
                    Quantity = vs.Quantity,
                    Status = vs.Status
                }).ToList()
            };
        }
        public async Task<PagedResult<TaskDailyModel>> GetTaskDailiesByGrowthStageIdAsync(TaskDailyFilterModel filter)
        {
            // Query cơ bản
            var query = _unitOfWork.TaskDailies
                .FindByCondition(td => td.GrowthStageId == filter.GrowthStageId)
                .AsQueryable();

            // Áp dụng bộ lọc
            if (!string.IsNullOrEmpty(filter.TaskName))
            {
                query = query.Where(td => td.TaskName.Contains(filter.TaskName));
            }

            if (filter.Session.HasValue)
            {
                query = query.Where(td => td.Session == filter.Session.Value);
            }

            // Tổng số phần tử
            var totalItems = await query.CountAsync();

            // Phân trang và lấy dữ liệu
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(td => new TaskDailyModel
                {
                    Id = td.Id,
                    GrowthStageId = td.GrowthStageId,
                    TaskTypeId = td.TaskTypeId,
                    TaskName = td.TaskName,
                    Description = td.Description,
                    Session = td.Session,
                    StartAt = td.StartAt,
                    EndAt = td.EndAt
                })
                .ToListAsync();

            var result = new PaginatedList<TaskDailyModel>(items, totalItems, filter.PageNumber, filter.PageSize);

            // Kết quả phân trang
            return new PagedResult<TaskDailyModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage,
            };
        }
        public async Task<PagedResult<VaccineScheduleModel>> GetVaccineSchedulesByGrowthStageIdAsync(VaccineScheduleFilterModel filter)
        {
            // Query cơ bản
            var query = _unitOfWork.VaccineSchedules
                .FindByCondition(vs => vs.StageId == filter.GrowthStageId)
                .AsQueryable();

            // Áp dụng bộ lọc trạng thái
            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(vs => vs.Status == filter.Status);
            }

            // Tổng số phần tử
            var totalItems = await query.CountAsync();

            // Phân trang và lấy dữ liệu
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(vs => new VaccineScheduleModel
                {
                    Id = vs.Id,
                    StageId = vs.StageId,
                    VaccineId = vs.VaccineId,
                    Date = vs.Date,
                    ApplicationAge = vs.ApplicationAge,
                    Quantity = vs.Quantity,
                    Status = vs.Status
                })
                .ToListAsync();

            var result = new PaginatedList<VaccineScheduleModel>(items, totalItems, filter.PageNumber, filter.PageSize);

            // Kết quả phân trang
            return new PagedResult<VaccineScheduleModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage,
            };
        }


        public async Task<GrowthStageDetailModel> GetActiveGrowthStageByCageIdAsync(Guid cageId)
        {
            // Tìm FarmingBatch với trạng thái "đang diễn ra"
            var farmingBatch = await _unitOfWork.FarmingBatches.FindByCondition(
                fb => fb.CageId == cageId && fb.Status == FarmingBatchStatusEnum.Active,
                trackChanges: false
            ).FirstOrDefaultAsync();

            if (farmingBatch == null)
                return null;

            // Tìm GrowthStage với trạng thái "đang diễn ra"
            var growthStage = await _unitOfWork.GrowthStages.FindByCondition(
                gs => gs.FarmingBatchId == farmingBatch.Id && gs.Status == GrowthStageStatusEnum.Active,
                trackChanges: false
            ).FirstOrDefaultAsync();

            if (growthStage == null)
                return null;

            // Map GrowthStage sang GrowthStageModel
            return new GrowthStageDetailModel
            {
                Id = growthStage.Id,
                Name = growthStage.Name,
                WeightAnimal = growthStage.WeightAnimal,
                Quantity = growthStage.Quantity,
                AgeStart = growthStage.AgeStart,
                AgeEnd = growthStage.AgeEnd,
                AgeStartDate = growthStage.AgeStartDate,
                AgeEndDate = growthStage.AgeEndDate,
                Status = growthStage.Status,
                RecommendedWeightPerSession = growthStage.RecommendedWeightPerSession,
                WeightBasedOnBodyMass = growthStage.WeightBasedOnBodyMass
            };
        }

    }
}
