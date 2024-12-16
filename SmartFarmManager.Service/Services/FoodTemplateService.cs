using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.FoodTemplate;
using SmartFarmManager.Service.BusinessModels.GrowthStageTemplate;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class FoodTemplateService:IFoodTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FoodTemplateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CreateFoodTemplateAsync(CreateFoodTemplateModel model)
        {
            var stageTemplate = await _unitOfWork.GrowthStageTemplates
                .FindByCondition(s => s.Id == model.StageTemplateId)
                .FirstOrDefaultAsync();

            if (stageTemplate == null)
            {
                throw new ArgumentException($"Growth Stage Template with ID {model.StageTemplateId} does not exist.");
            }

            var duplicateFoodNameExists = await _unitOfWork.FoodTemplates
                .FindByCondition(f => f.StageTemplateId == model.StageTemplateId && f.FoodName == model.FoodName)
                .AnyAsync();

            if (duplicateFoodNameExists)
            {
                throw new InvalidOperationException($"Food with name '{model.FoodName}' already exists in the specified Growth Stage Template.");
            }

            var foodTemplate = new FoodTemplate
            {
                StageTemplateId = model.StageTemplateId,
                FoodName = model.FoodName,
                RecommendedWeightPerDay = model.RecommendedWeightPerDay,
                Session = model.Session,
                WeightBasedOnBodyMass = model.WeightBasedOnBodyMass
            };

            await _unitOfWork.FoodTemplates.CreateAsync(foodTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> UpdateFoodTemplateAsync(UpdateFoodTemplateModel model)
        {
            // 1. Tìm FoodTemplate cần cập nhật
            var foodTemplate = await _unitOfWork.FoodTemplates
                .FindByCondition(f => f.Id == model.Id)
                .FirstOrDefaultAsync();

            if (foodTemplate == null)
            {
                return false; // Không tìm thấy
            }

            // 2. Cập nhật các trường nếu có giá trị mới
            if (!string.IsNullOrEmpty(model.FoodName) && foodTemplate.FoodName != model.FoodName)
            {
                // Kiểm tra trùng tên FoodName trong cùng GrowthStageTemplate
                var duplicateFoodNameExists = await _unitOfWork.FoodTemplates
                    .FindByCondition(f => f.StageTemplateId == foodTemplate.StageTemplateId && f.FoodName == model.FoodName && f.Id != model.Id)
                    .AnyAsync();

                if (duplicateFoodNameExists)
                {
                    throw new InvalidOperationException($"Food with name '{model.FoodName}' already exists in the specified Growth Stage Template.");
                }

                foodTemplate.FoodName = model.FoodName;
            }

            if (model.RecommendedWeightPerDay.HasValue)
            {
                foodTemplate.RecommendedWeightPerDay = model.RecommendedWeightPerDay.Value;
            }

            if (model.Session.HasValue)
            {
                foodTemplate.Session = model.Session.Value;
            }

            if (model.WeightBasedOnBodyMass.HasValue)
            {
                foodTemplate.WeightBasedOnBodyMass = model.WeightBasedOnBodyMass.Value;
            }

            // 3. Lưu thay đổi
            await _unitOfWork.FoodTemplates.UpdateAsync(foodTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> DeleteFoodTemplateAsync(Guid id)
        {
            var foodTemplate = await _unitOfWork.FoodTemplates
                .FindByCondition(f => f.Id == id)
                .FirstOrDefaultAsync();

            if (foodTemplate == null)
            {
                return false;
            }

            await _unitOfWork.FoodTemplates.DeleteAsync(foodTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<PagedResult<FoodTemplateItemModel>> GetFoodTemplatesAsync(FoodTemplateFilterModel filter)
        {
            var query = _unitOfWork.FoodTemplates.FindAll(false).AsQueryable();

            if (filter.StageTemplateId.HasValue)
            {
                query = query.Where(f => f.StageTemplateId == filter.StageTemplateId.Value);
            }

            if (!string.IsNullOrEmpty(filter.FoodName))
            {
                query = query.Where(f => f.FoodName.Contains(filter.FoodName));
            }

            if (filter.Session.HasValue)
            {
                query = query.Where(f => f.Session == filter.Session.Value);
            }
            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(f => f.FoodName)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(f => new FoodTemplateItemModel
                {
                    Id = f.Id,
                    StageTemplateId = f.StageTemplateId,
                    FoodName = f.FoodName,
                    RecommendedWeightPerDay = f.RecommendedWeightPerDay,
                    Session = f.Session,
                    WeightBasedOnBodyMass = f.WeightBasedOnBodyMass
                })
                .ToListAsync();

            var result = new PaginatedList<FoodTemplateItemModel>(items,totalItems,filter.PageNumber,filter.PageSize);

            return new PagedResult<FoodTemplateItemModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage= result.HasNextPage,
                HasPreviousPage= result.HasPreviousPage,
            };
        }

        public async Task<FoodTemplateDetailModel?> GetFoodTemplateDetailAsync(Guid id)
        {
            // 1. Tìm FoodTemplate theo id
            var foodTemplate = await _unitOfWork.FoodTemplates
                .FindByCondition(f => f.Id == id)
                .Include(f => f.StageTemplate) 
                .FirstOrDefaultAsync();

            if (foodTemplate == null)
            {
                return null;
            }
            // 2. Map dữ liệu vào response
            return new FoodTemplateDetailModel
            {
                Id = foodTemplate.Id,
                StageTemplateId = foodTemplate.StageTemplateId,
                FoodName = foodTemplate.FoodName,
                RecommendedWeightPerDay = foodTemplate.RecommendedWeightPerDay,
                Session = foodTemplate.Session,
                WeightBasedOnBodyMass = foodTemplate.WeightBasedOnBodyMass,
                GrowthStageTemplate = foodTemplate.StageTemplate == null ? null : new GrowthStageTemplateResponse
                {
                    Id = foodTemplate.StageTemplate.Id,
                    StageName = foodTemplate.StageTemplate.StageName,
                    WeightAnimal = foodTemplate.StageTemplate.WeightAnimal,
                    AgeStart = foodTemplate.StageTemplate.AgeStart,
                    AgeEnd = foodTemplate.StageTemplate.AgeEnd,
                    Notes = foodTemplate.StageTemplate.Notes
                }
            };
        }



    }
}
