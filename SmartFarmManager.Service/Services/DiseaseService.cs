using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Disease;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFarmManager.DataAccessObject.Models;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Service.Helpers;

namespace SmartFarmManager.Service.Services
{
    public class DiseaseService : IDiseaseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DiseaseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<DiseaseModel>> GetPagedDiseasesAsync(string? name, int page, int pageSize)
        {
            var (items, totalCount) = await _unitOfWork.Diseases.GetPagedAsync(
                filter: d => string.IsNullOrEmpty(name) || d.Name.Contains(name),
                orderBy: q => q.OrderBy(d => d.Name),
                page: page,
                pageSize: pageSize
            );

            return new PagedResult<DiseaseModel>
            {
                Items = items.Select(d => new DiseaseModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                }),
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                HasNextPage = page < (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = page > 1
            };
        }


        public async Task<bool> CreateDiseaseAsync(CreateDiseaseModel model)
        {
            var existingDisease = await _unitOfWork.Diseases
                .FindByCondition(d => d.Name == model.Name)
                .FirstOrDefaultAsync();

            if (existingDisease != null)
            {
                throw new ArgumentException($"Disease with name '{model.Name}' already exists.");
            }

            var disease = new Disease
            {
                Name = model.Name,
                Description = model.Description
            };

            await _unitOfWork.Diseases.CreateAsync(disease);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> UpdateDiseaseAsync(Guid id, UpdateDiseaseModel model)
        {
            var existingDisease = await _unitOfWork.Diseases
                .FindByCondition(d => d.Id == id)
                .FirstOrDefaultAsync();

            if (existingDisease == null)
            {
                throw new KeyNotFoundException($"Disease with ID {id} does not exist.");
            }

            existingDisease.Name = model.Name ?? existingDisease.Name;
            existingDisease.Description = model.Description ?? existingDisease.Description;

            await _unitOfWork.Diseases.UpdateAsync(existingDisease);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<bool> DeleteDiseaseAsync(Guid id)
        {
            var existingDisease = await _unitOfWork.Diseases
                .FindByCondition(d => d.Id == id)
                .FirstOrDefaultAsync();

            if (existingDisease == null)
            {
                throw new KeyNotFoundException($"Disease with ID {id} does not exist.");
            }

            if (existingDisease.IsDeleted)
            {
                var diseaseWithSameName = await _unitOfWork.Diseases
                    .FindByCondition(d => d.Name == existingDisease.Name && d.IsDeleted == false)
                    .FirstOrDefaultAsync();

                if (diseaseWithSameName != null)
                {
                    throw new InvalidOperationException($"Bệnh với tên '{existingDisease.Name}' đã tồn tại và không thể khổi phục");
                }

                existingDisease.IsDeleted = false;
            }
            else
            {
                existingDisease.IsDeleted = true;
            }

            await _unitOfWork.Diseases.UpdateAsync(existingDisease);
            await _unitOfWork.CommitAsync();

            if (existingDisease.IsDeleted)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<PagedResult<DiseaseItemModel>> GetDiseasesAsync(DiseaseFilterModel filter)
        {
            var query = _unitOfWork.Diseases.FindAll(false).AsQueryable();

            
            if (!string.IsNullOrEmpty(filter.KeySearch))
            {
                query = query.Where(d => d.Name.Contains(filter.KeySearch));
            }
            if(filter.IsDeleted.HasValue)
            {
                query = query.Where(d => d.IsDeleted == filter.IsDeleted);
            }
           

            var totalItems = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(d => new DiseaseItemModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    IsDeleted = d.IsDeleted

                })
                .ToListAsync();

            var result = new PaginatedList<DiseaseItemModel>(items, totalItems, filter.PageNumber, filter.PageSize);
            return new PagedResult<DiseaseItemModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage
            };
        }

        public async Task<DiseaseDetailResponseModel> GetDiseaseDetailAsync(Guid id)
        {
            var disease = await _unitOfWork.Diseases
                .FindByCondition(d => d.Id == id)
                .FirstOrDefaultAsync();

            if (disease == null)
            {
                return null;
            }

            return new DiseaseDetailResponseModel
            {
                Id = disease.Id,
                Name = disease.Name,
                Description = disease.Description
            };
        }

    }
}
