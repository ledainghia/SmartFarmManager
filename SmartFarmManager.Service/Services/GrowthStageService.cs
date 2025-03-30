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
using SmartFarmManager.Service.BusinessModels.AnimalSale;

namespace SmartFarmManager.Service.Services
{
    public class GrowthStageService : IGrowthStageService
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
                FarmingBatchId = farmingBatch.Id,
                Name = growthStage.Name,
                WeightAnimal = growthStage.WeightAnimal,
                Quantity = growthStage.Quantity,
                AffectQuantity = growthStage.AffectedQuantity,
                DeadQuantity = growthStage.DeadQuantity,
                AgeStart = growthStage.AgeStart,
                AgeEnd = growthStage.AgeEnd,
                AgeStartDate = growthStage.AgeStartDate,
                AgeEndDate = growthStage.AgeEndDate,
                Status = growthStage.Status,
                RecommendedWeightPerSession = growthStage.RecommendedWeightPerSession,
                WeightBasedOnBodyMass = growthStage.WeightBasedOnBodyMass,
                FoodType = growthStage.FoodType,
            };
        }
        public async Task<bool> UpdateWeightAnimalAsync(UpdateGrowthStageRequest request)
        {
            // 1️⃣ Tìm GrowthStage theo ID
            var growthStage = await _unitOfWork.GrowthStages
                .FindByCondition(gs => gs.Id == request.GrowthStageId)
                .FirstOrDefaultAsync();

            if (growthStage == null)
                return false; // Không tìm thấy GrowthStage

            // 2️⃣ Cập nhật WeightAnimal với giá trị mới từ request
            growthStage.WeightAnimal = request.WeightAnimal;

            // 3️⃣ Tính lại RecommendedWeightPerSession
            if (growthStage.WeightBasedOnBodyMass.HasValue)
            {
                growthStage.RecommendedWeightPerSession = request.WeightAnimal * growthStage.WeightBasedOnBodyMass.Value;
            }

            // 4️⃣ Cập nhật lại GrowthStage trong database
            await _unitOfWork.GrowthStages.UpdateAsync(growthStage);
            await _unitOfWork.CommitAsync();

            return true; // Cập nhật thành công
        }

        public async Task UpdateGrowthStagesStatusAsync()
        {
            var activeFarmingBatches = await _unitOfWork.FarmingBatches
           .FindByCondition(fb => fb.Status == FarmingBatchStatusEnum.Active)
           .ToListAsync();

            if (!activeFarmingBatches.Any())
            {
                throw new Exception("No active farming batches found.");
            }

            // Lấy ngày hôm nay
            var today = DateTimeUtils.GetServerTimeInVietnamTime().Date;
            foreach (var farmingBatch in activeFarmingBatches)
            {
                // Lấy tất cả các GrowthStage của FarmingBatch này
                var growthStages = await _unitOfWork.GrowthStages
                    .FindByCondition(gs => gs.FarmingBatchId == farmingBatch.Id)
                    .OrderBy(gs => gs.AgeStartDate)  // Sắp xếp theo AgeStartDate
                    .ToListAsync();

                if (!growthStages.Any())
                {
                    throw new Exception($"No growth stages found for farming batch {farmingBatch.Id}.");
                }

                for (int i = 0; i < growthStages.Count; i++)
                {
                    var currentGrowthStage = growthStages[i];

                    // Kiểm tra nếu hôm nay là ngày kết thúc của giai đoạn này
                    if (currentGrowthStage.Status==GrowthStageStatusEnum.Active && currentGrowthStage.AgeEndDate.HasValue && currentGrowthStage.AgeEndDate.Value.Date == today)
                    {
                        // Cập nhật trạng thái của giai đoạn hiện tại thành Completed
                        currentGrowthStage.Status = "Completed";
                        await _unitOfWork.GrowthStages.UpdateAsync(currentGrowthStage);
                    }
                    else
                    {
                        // Nếu chưa tới ngày kết thúc, bỏ qua giai đoạn này
                        continue;
                    }

                    if (i + 1 < growthStages.Count)
                    {
                        var nextGrowthStage = growthStages[i + 1];

                        // Nếu giai đoạn tiếp theo có trạng thái "Upcoming" và AgeStart = AgeEnd của giai đoạn hiện tại + 1
                        if (nextGrowthStage.Status == GrowthStageStatusEnum.Upcoming &&                 
                            nextGrowthStage.AgeStart == currentGrowthStage.AgeEnd+1)
                        {
                            // Cập nhật trạng thái của giai đoạn tiếp theo thành Active
                            nextGrowthStage.Status = GrowthStageStatusEnum.Active;

                            // Cập nhật Quantity và AffectedQuantity cho giai đoạn mới
                            nextGrowthStage.Quantity = currentGrowthStage.Quantity.GetValueOrDefault() - currentGrowthStage.DeadQuantity.GetValueOrDefault();
                            nextGrowthStage.AffectedQuantity = 0;
                            // Cập nhật giai đoạn phát triển tiếp theo
                            await _unitOfWork.GrowthStages.UpdateAsync(nextGrowthStage);
                        }
                    }

                    await _unitOfWork.CommitAsync();
                }
            }
        }
        public async Task<List<AnimalSaleGroupedByTypeModel>> GetAnimalSalesByGrowthStageAsync(Guid growthStageId)
        {
            // 1️⃣ Lấy giai đoạn phát triển
            var growthStage = await _unitOfWork.GrowthStages
                .FindByCondition(gs => gs.Id == growthStageId)
                .Include(gs => gs.FarmingBatch)
                .FirstOrDefaultAsync();

            if (growthStage == null || !growthStage.AgeStartDate.HasValue)
                throw new ArgumentException("Không tìm thấy giai đoạn phát triển hợp lệ.");

            var farmingBatchId = growthStage.FarmingBatchId;
            var startDate = growthStage.AgeStartDate.Value;

            // 2️⃣ Lấy tất cả các giai đoạn khác của vụ nuôi để xác định có phải là giai đoạn cuối không
            var allStages = await _unitOfWork.GrowthStages
                .FindByCondition(gs => gs.FarmingBatchId == farmingBatchId)
                .ToListAsync();

            var isLastStage = !allStages.Any(gs => gs.AgeStartDate > growthStage.AgeStartDate);
            DateTime? endDate = isLastStage ? null : growthStage.AgeEndDate;

            // 3️⃣ Lọc các bản ghi AnimalSales trong giai đoạn đó
            var query = _unitOfWork.AnimalSales
                .FindByCondition(s => s.FarmingBatchId == farmingBatchId && s.SaleDate >= startDate);

            if (endDate.HasValue)
            {
                query = query.Where(s => s.SaleDate <= endDate.Value);
            }

            var animalSales = await query
                .Include(s => s.SaleType)
                .ToListAsync();

            // 4️⃣ Group theo SaleType
            var grouped = animalSales
                .GroupBy(s => s.SaleType.StageTypeName)
                .Select(g => new AnimalSaleGroupedByTypeModel
                {
                    SaleType = g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Total),
                    UnitPriceAverage = g.Average(x => x.UnitPrice ?? 0),
                    Logs = g.Select(x => new AnimalSaleLogModel
                    {
                        SaleDate = x.SaleDate,
                        Quantity = x.Quantity,
                        UnitPrice = x.UnitPrice,
                        Total = x.Total
                    }).ToList()
                })
                .ToList();

            return grouped;
        }




    }
}
