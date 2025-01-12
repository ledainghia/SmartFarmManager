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
            // 1. Kiểm tra GrowthStageTemplate có tồn tại
            var stageTemplate = await _unitOfWork.GrowthStageTemplates
                .FindByCondition(s => s.Id == model.StageTemplateId)
                .FirstOrDefaultAsync();

            if (stageTemplate == null)
            {
                throw new ArgumentException($"Giai đoạn phát triển với ID '{model.StageTemplateId}' không tồn tại.");
            }

            // 2. Kiểm tra trùng lặp FoodType trong cùng GrowthStageTemplate
            var duplicateFoodNameExists = await _unitOfWork.FoodTemplates
                .FindByCondition(f => f.StageTemplateId == model.StageTemplateId && f.FoodType == model.FoodType)
                .AnyAsync();

            if (duplicateFoodNameExists)
            {
                throw new InvalidOperationException($"Loại thức ăn '{model.FoodType}' đã tồn tại trong giai đoạn phát triển được chỉ định.");
            }

            // 3. Kiểm tra loại thức ăn có tồn tại trong kho không
            var foodInStock = await _unitOfWork.FoodStacks
         .FindByCondition(fs => fs.FoodType == model.FoodType)
         .FirstOrDefaultAsync();

            if (foodInStock == null)
            {
                throw new ArgumentException($"Loại thức ăn '{model.FoodType}' không tồn tại trong kho của farm hiện tại.");
            }

            // 4. Tạo FoodTemplate
            var foodTemplate = new FoodTemplate
            {
                StageTemplateId = model.StageTemplateId,
                FoodType = model.FoodType,
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
            if (!string.IsNullOrEmpty(model.FoodType) && foodTemplate.FoodType != model.FoodType)
            {
                // Kiểm tra trùng tên FoodName trong cùng GrowthStageTemplate
                var duplicateFoodNameExists = await _unitOfWork.FoodTemplates
                    .FindByCondition(f => f.StageTemplateId == foodTemplate.StageTemplateId && f.FoodType == model.FoodType && f.Id != model.Id)
                    .AnyAsync();

                if (duplicateFoodNameExists)
                {
                    throw new InvalidOperationException($"Food with name '{model.FoodType}' already exists in the specified Growth Stage Template.");
                }

                foodTemplate.FoodType = model.FoodType;
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

            if (!string.IsNullOrEmpty(filter.FoodType))
            {
                query = query.Where(f => f.FoodType.Contains(filter.FoodType));
            }

         
            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(f => f.FoodType)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(f => new FoodTemplateItemModel
                {
                    Id = f.Id,
                    StageTemplateId = f.StageTemplateId,
                    FoodType = f.FoodType,
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
                FoodType = foodTemplate.FoodType,
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
