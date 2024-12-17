    using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.AnimalTemplate;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.GrowthStageTemplate;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFarmManager.Service.Helpers;

namespace SmartFarmManager.Service.Services
{
    public class GrowthStageTemplateService:IGrowthStageTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GrowthStageTemplateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateGrowthStageTemplateAsync(CreateGrowthStageTemplateModel model)
        {
            var animalTemplate = await _unitOfWork.AnimalTemplates.FindByCondition(t => t.Id == model.TemplateId).FirstOrDefaultAsync();
            if (animalTemplate == null)
            {
                throw new ArgumentException($"Animal Template with ID {model.TemplateId} does not exist.");
            }

            var duplicateStageName = await _unitOfWork.GrowthStageTemplates
                .FindByCondition(s => s.TemplateId == model.TemplateId && s.StageName == model.StageName)
                .AnyAsync();

            if (duplicateStageName)
            {
                throw new ArgumentException($"StageName '{model.StageName}' already exists in the Animal Template.");
            }

            var growthStageTemplate = new GrowthStageTemplate
            {
                TemplateId = model.TemplateId,
                StageName = model.StageName,
                WeightAnimal = model.WeightAnimal,
                AgeStart = model.AgeStart,
                AgeEnd = model.AgeEnd,
                Notes = model.Notes
            };

            await _unitOfWork.GrowthStageTemplates.CreateAsync(growthStageTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<bool> UpdateGrowthStageTemplateAsync(Guid id, UpdateGrowthStageTemplateModel model)
        {
            // 1. Kiểm tra GrowthStageTemplate có tồn tại không
            var existingTemplate = await _unitOfWork.GrowthStageTemplates.FindByCondition(t => t.Id == id).FirstOrDefaultAsync();
            if (existingTemplate == null)
            {
                throw new KeyNotFoundException($"Growth Stage Template with ID {id} does not exist.");
            }

            // 2. Kiểm tra StageName (nếu có thay đổi)
            if (!string.IsNullOrEmpty(model.StageName) && model.StageName != existingTemplate.StageName)
            {
                var duplicateStageName = await _unitOfWork.GrowthStageTemplates
                    .FindByCondition(s => s.TemplateId == existingTemplate.TemplateId && s.StageName == model.StageName)
                    .AnyAsync();

                if (duplicateStageName)
                {
                    throw new ArgumentException($"StageName '{model.StageName}' already exists in the Animal Template.");
                }

                existingTemplate.StageName = model.StageName;
            }

            if (model.WeightAnimal.HasValue)
            {
                existingTemplate.WeightAnimal = model.WeightAnimal.Value;
            }

            if (model.AgeStart.HasValue)
            {
                existingTemplate.AgeStart = model.AgeStart.Value;
            }

            if (model.AgeEnd.HasValue)
            {
                existingTemplate.AgeEnd = model.AgeEnd.Value;
            }

            if (!string.IsNullOrEmpty(model.Notes))
            {
                existingTemplate.Notes = model.Notes;
            }

            // 4. Lưu thay đổi
            await _unitOfWork.GrowthStageTemplates.UpdateAsync(existingTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> DeleteGrowthStageTemplateAsync(Guid id)
        {
            // 1. Kiểm tra GrowthStageTemplate có tồn tại không
            var existingTemplate = await _unitOfWork.GrowthStageTemplates
                .FindByCondition(t => t.Id == id)
                .FirstOrDefaultAsync();

            if (existingTemplate == null)
            {
                throw new KeyNotFoundException($"Growth Stage Template with ID {id} does not exist.");
            }

            await _unitOfWork.GrowthStageTemplates.DeleteAsync(existingTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<PagedResult<GrowthStageTemplateItemModel>> GetGrowthStageTemplatesAsync(GrowthStageTemplateFilterModel filter)
        {
            // Query cơ bản
            var query = _unitOfWork.GrowthStageTemplates.FindAll(false).AsQueryable();

            // Áp dụng bộ lọc
            if (filter.TemplateId.HasValue)
            {
                query = query.Where(g => g.TemplateId == filter.TemplateId.Value);
            }

            if (!string.IsNullOrEmpty(filter.StageName))
            {
                query = query.Where(g => g.StageName.Contains(filter.StageName));
            }

            if (filter.AgeStart.HasValue)
            {
                query = query.Where(g => g.AgeStart >= filter.AgeStart.Value);
            }

            if (filter.AgeEnd.HasValue)
            {
                query = query.Where(g => g.AgeEnd <= filter.AgeEnd.Value);
            }

            // Tổng số phần tử
            var totalItems = await query.CountAsync();

            // Phân trang và lấy dữ liệu
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(g => new GrowthStageTemplateItemModel
                {
                    Id = g.Id,
                    TemplateId = g.TemplateId,
                    StageName = g.StageName,
                    WeightAnimal = g.WeightAnimal,
                    AgeStart = g.AgeStart,
                    AgeEnd = g.AgeEnd,
                    Notes = g.Notes
                })
                .ToListAsync();

            var result = new PaginatedList<GrowthStageTemplateItemModel>(items, totalItems, filter.PageNumber, filter.PageSize);

            // Kết quả phân trang
            return new PagedResult<GrowthStageTemplateItemModel>
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

        public async Task<GrowthStageTemplateDetailResponseModel?> GetGrowthStageTemplateDetailAsync(Guid id)
        {
            var growthStageTemplate = await _unitOfWork.GrowthStageTemplates
                .FindByCondition(g => g.Id == id)
                .Include(g => g.FoodTemplates)
                .Include(g => g.TaskDailyTemplates) 
                .FirstOrDefaultAsync();

            if (growthStageTemplate == null)
            {
                return null;
            }
            var taskTypeIds = growthStageTemplate.TaskDailyTemplates
                .Where(td => td.TaskTypeId.HasValue)
                .Select(td => td.TaskTypeId.Value)
                .Distinct()
                .ToList();

            var taskTypes = await _unitOfWork.TaskTypes
                .FindByCondition(tt => taskTypeIds.Contains(tt.Id))
                .ToDictionaryAsync(tt => tt.Id, tt => new TaskTypeResponse
                {
                    Id = tt.Id,
                    TaskTypeName = tt.TaskTypeName,
                    PriorityNum = tt.PriorityNum
                });

            // Map dữ liệu vào response
            return new GrowthStageTemplateDetailResponseModel
            {
                Id = growthStageTemplate.Id,
                TemplateId = growthStageTemplate.TemplateId,
                StageName = growthStageTemplate.StageName,
                WeightAnimal = growthStageTemplate.WeightAnimal,
                AgeStart = growthStageTemplate.AgeStart,
                AgeEnd = growthStageTemplate.AgeEnd,
                Notes = growthStageTemplate.Notes,
                FoodTemplates = growthStageTemplate.FoodTemplates.Select(f => new FoodTemplateResponse
                {
                    Id = f.Id,
                    FoodName = f.FoodName,
                    RecommendedWeightPerDay = f.RecommendedWeightPerDay,
                    Session = f.Session,
                    WeightBasedOnBodyMass = f.WeightBasedOnBodyMass
                }).ToList(),
                TaskDailyTemplates = growthStageTemplate.TaskDailyTemplates.Select(td => new TaskDailyTemplateResponse
                {
                    Id = td.Id,
                    TaskName = td.TaskName,
                    Description = td.Description,
                    Session = td.Session,
                    TaskType = td.TaskTypeId.HasValue && taskTypes.ContainsKey(td.TaskTypeId.Value)
                        ? taskTypes[td.TaskTypeId.Value]
                        : null
                }).ToList()
            };
        }


    }
}
