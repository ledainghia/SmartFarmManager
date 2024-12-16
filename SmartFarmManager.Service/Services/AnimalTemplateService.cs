﻿using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.AnimalTemplate;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class AnimalTemplateService:IAnimalTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AnimalTemplateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
        }

        public async Task<bool> CreateAnimalTemplateAsync(CreateAnimalTemplateModel model)
        {
          

            // 2. Check for Duplicate Name and Species
            var duplicateExists = await _unitOfWork.AnimalTemplates
                .FindByCondition(x => x.Name == model.Name)
                .AnyAsync();

            if (duplicateExists)
            {
                throw new InvalidOperationException($"An Animal Template with the name '{model.Name}' already exists.");
            }

            // 3. Create Animal Template
            var newTemplate = new AnimalTemplate
            {
                Name = model.Name,
                Species = model.Species,
                DefaultCapacity = model.DefaultCapacity,
                Status = AnimalTemplateStatusEnum.Draft,
                Notes = model.Notes,
            };

            await _unitOfWork.AnimalTemplates.CreateAsync(newTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> UpdateAnimalTemplateAsync(Guid id, UpdateAnimalTemplateModel model)
        {
            // 1. Kiểm tra AnimalTemplate có tồn tại không
            var existingTemplate = await _unitOfWork.AnimalTemplates.FindByCondition(t => t.Id == id).FirstOrDefaultAsync();
            if (existingTemplate == null)
            {
                throw new ArgumentException($"Animal Template with ID {id} does not exist.");
            }

            // 2. Kiểm tra giá trị Name
            if (!string.IsNullOrEmpty(model.Name))
            {
                // Kiểm tra trùng tên với template khác
                var duplicateNameExists = await _unitOfWork.AnimalTemplates
                    .FindByCondition(t => t.Name == model.Name && t.Id != id)
                    .AnyAsync();

                if (duplicateNameExists)
                {
                    throw new InvalidOperationException($"An Animal Template with the name '{model.Name}' already exists.");
                }

                existingTemplate.Name = model.Name;
            }

            // 3. Kiểm tra giá trị Species
            if (!string.IsNullOrEmpty(model.Species))
            {
                // Kiểm tra trùng Species
                var duplicateSpeciesExists = await _unitOfWork.AnimalTemplates
                    .FindByCondition(t => t.Name == model.Species && t.Id != id)
                    .AnyAsync();

                if (duplicateSpeciesExists)
                {
                    throw new InvalidOperationException($"An Animal Template with the name '{model.Name}' already exists.");
                }

                existingTemplate.Species = model.Species;
            }

            // 4. Kiểm tra giá trị DefaultCapacity
            if (model.DefaultCapacity.HasValue)
            {
                if (model.DefaultCapacity <= 0)
                {
                    throw new ArgumentException("Default Capacity must be greater than zero.");
                }

                existingTemplate.DefaultCapacity = model.DefaultCapacity;
            }

            // 5. Kiểm tra giá trị Notes
            if (!string.IsNullOrEmpty(model.Notes))
            {
                existingTemplate.Notes = model.Notes;
            }

            await _unitOfWork.AnimalTemplates.UpdateAsync(existingTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> ChangeStatusAsync(Guid id, string newStatus)
        {
            var existingTemplate = await _unitOfWork.AnimalTemplates.FindByCondition(t => t.Id == id).FirstOrDefaultAsync();
            if (existingTemplate == null)
            {
                throw new KeyNotFoundException($"Animal Template with ID {id} does not exist.");
            }

            var validStatuses = new List<string>
    {
        AnimalTemplateStatusEnum.Draft,
        AnimalTemplateStatusEnum.Active,
        AnimalTemplateStatusEnum.Inactive
    };

            if (!validStatuses.Contains(newStatus))
            {
                throw new ArgumentException($"Invalid status value: {newStatus}");
            }

            if (string.Equals(existingTemplate.Status, newStatus, StringComparison.OrdinalIgnoreCase))
            {
                return true; 
            }

            existingTemplate.Status = newStatus;
            await _unitOfWork.AnimalTemplates.UpdateAsync(existingTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<bool> DeleteAnimalTemplateAsync(Guid id)
        {
            var existingTemplate = await _unitOfWork.AnimalTemplates.FindByCondition(t => t.Id == id).FirstOrDefaultAsync();
            if (existingTemplate == null)
            {
                throw new KeyNotFoundException($"Animal Template with ID {id} does not exist.");
            }

            if (string.Equals(existingTemplate.Status, AnimalTemplateStatusEnum.Deleted, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            existingTemplate.Status = AnimalTemplateStatusEnum.Deleted;
            await _unitOfWork.AnimalTemplates.UpdateAsync(existingTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }


        public async Task<PagedResult<AnimalTemplateItemModel>> GetFilteredAnimalTemplatesAsync(AnimalTemplateFilterModel filter)
        {
            // Query cơ bản
            var query = _unitOfWork.AnimalTemplates.FindAll(false).AsQueryable();

            // Lọc các template có Status khác Deleted
            query = query.Where(t => !t.Status.Equals(AnimalTemplateStatusEnum.Deleted, StringComparison.OrdinalIgnoreCase));

            // Áp dụng các bộ lọc từ request
            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(t => t.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrEmpty(filter.Species))
            {
                query = query.Where(t => t.Species.Contains(filter.Species));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(t => t.Status.Equals(filter.Status, StringComparison.OrdinalIgnoreCase));
            }

            // Tổng số phần tử
            var totalItems = await query.CountAsync();



            // Phân trang và lấy dữ liệu
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(t => new AnimalTemplateItemModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Species = t.Species,
                    Status = t.Status,
                    DefaultCapacity = t.DefaultCapacity,
                    Notes = t.Notes
                })
                .ToListAsync();
            var result = new PaginatedList<AnimalTemplateItemModel>(items,totalItems,filter.PageNumber,filter.PageSize);

            // Kết quả phân trang
            return new PagedResult<AnimalTemplateItemModel>
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

        public async Task<AnimalTemplateDetailResponseModel> GetAnimalTemplateDetailAsync(Guid id)
        {
            // Lấy AnimalTemplate cùng với GrowthStageTemplates và VaccineTemplates
            var animalTemplate = await _unitOfWork.AnimalTemplates
                .FindByCondition(x => x.Id == id)
                .Include(x => x.GrowthStageTemplates)
                .Include(x => x.VaccineTemplates)
                .FirstOrDefaultAsync();

            if (animalTemplate == null)
            {
                return null;
            }

            // Map dữ liệu vào response
            return new AnimalTemplateDetailResponseModel
            {
                Id = animalTemplate.Id,
                Name = animalTemplate.Name,
                Species = animalTemplate.Species,
                Status = animalTemplate.Status,
                DefaultCapacity = animalTemplate.DefaultCapacity,
                Notes = animalTemplate.Notes,
                GrowthStageTemplates = animalTemplate.GrowthStageTemplates.Select(g => new GrowthStageTemplateResponse
                {
                    Id = g.Id,
                    TemplateId = g.TemplateId,
                    StageName = g.StageName,
                    WeightAnimal = g.WeightAnimal,
                    AgeStart = g.AgeStart,
                    AgeEnd = g.AgeEnd,
                    Notes = g.Notes,
                }).ToList(),
                VaccineTemplates = animalTemplate.VaccineTemplates.Select(v => new VaccineTemplateResponse
                {
                    Id = v.Id,
                    TemplateId = v.TemplateId,
                    VaccineName = v.VaccineName,
                    ApplicationMethod = v.ApplicationMethod,
                    ApplicationAge = v.ApplicationAge,
                    Session = v.Session
                }).ToList()
            };
        }


    }
}