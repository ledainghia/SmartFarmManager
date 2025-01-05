using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.AnimalTemplate;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.VaccineTemplate;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFarmManager.Service.Helpers;

namespace SmartFarmManager.Service.Services
{
    public class VaccineTemplateService : IVaccineTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;

        public VaccineTemplateService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<bool> CreateVaccineTemplateAsync(CreateVaccineTemplateModel model)
        {
            // 1. Kiểm tra AnimalTemplate có tồn tại
            var animalTemplate = await _unitOfWork.AnimalTemplates
                .FindByCondition(a => a.Id == model.TemplateId)
                .FirstOrDefaultAsync();

            if (animalTemplate == null)
            {
                throw new ArgumentException($"Animal Template with ID {model.TemplateId} does not exist.");
            }

            // 2. Kiểm tra VaccineName trùng lặp
            var duplicateVaccineNameExists = await _unitOfWork.VaccineTemplates
                .FindByCondition(v => v.TemplateId == model.TemplateId && v.VaccineName == model.VaccineName)
                .AnyAsync();

            if (duplicateVaccineNameExists)
            {
                throw new InvalidOperationException($"Vaccine with name '{model.VaccineName}' already exists in the specified Animal Template.");
            }

            // 3. Kiểm tra ApplicationAge nằm trong khoảng AgeStart và AgeEnd của Vaccine
            var vaccine = await _unitOfWork.Vaccines
                .FindByCondition(v => v.Name == model.VaccineName)
                .FirstOrDefaultAsync();

            if (vaccine == null)
            {
                throw new ArgumentException($"Vaccine '{model.VaccineName}' does not exist.");
            }

            if (model.ApplicationAge < vaccine.AgeStart || model.ApplicationAge > vaccine.AgeEnd)
            {
                throw new InvalidOperationException(
                    $"ApplicationAge {model.ApplicationAge} must be between {vaccine.AgeStart} and {vaccine.AgeEnd} for Vaccine '{model.VaccineName}'.");
            }

            // 4. Tạo VaccineTemplate
            var vaccineTemplate = new VaccineTemplate
            {
                TemplateId = model.TemplateId,
                VaccineName = model.VaccineName,
                ApplicationMethod = model.ApplicationMethod,
                ApplicationAge = model.ApplicationAge,
                Session = model.Session
            };

            await _unitOfWork.VaccineTemplates.CreateAsync(vaccineTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }


        public async Task<bool> UpdateVaccineTemplateAsync(UpdateVaccineTemplateModel model)
        {
            // 1. Kiểm tra VaccineTemplate có tồn tại
            var vaccineTemplate = await _unitOfWork.VaccineTemplates
                .FindByCondition(v => v.Id == model.Id)
                .FirstOrDefaultAsync();

            if (vaccineTemplate == null)
            {
                return false; // Không tìm thấy
            }

            // 2. Kiểm tra trùng lặp VaccineName (nếu có thay đổi VaccineName)
            if (!string.IsNullOrEmpty(model.VaccineName) && vaccineTemplate.VaccineName != model.VaccineName)
            {
                var duplicateVaccineNameExists = await _unitOfWork.VaccineTemplates
                    .FindByCondition(v => v.TemplateId == vaccineTemplate.TemplateId
                                          && v.VaccineName == model.VaccineName
                                          && v.Id != model.Id)
                    .AnyAsync();

                if (duplicateVaccineNameExists)
                {
                    throw new InvalidOperationException($"Vaccine with name '{model.VaccineName}' already exists in the specified Animal Template.");
                }

                vaccineTemplate.VaccineName = model.VaccineName;
            }

            // 3. Cập nhật ApplicationMethod (nếu có)
            if (!string.IsNullOrEmpty(model.ApplicationMethod))
            {
                vaccineTemplate.ApplicationMethod = model.ApplicationMethod;
            }

            if (model.ApplicationAge.HasValue)
            {

                var vaccine = await _unitOfWork.Vaccines
                    .FindByCondition(v => v.Name == vaccineTemplate.VaccineName)
                    .FirstOrDefaultAsync();

                if (vaccine == null)
                {
                    throw new ArgumentException($"Vaccine '{vaccineTemplate.VaccineName}' does not exist.");
                }

                if (model.ApplicationAge < vaccine.AgeStart || model.ApplicationAge > vaccine.AgeEnd)
                {
                    throw new InvalidOperationException(
                        $"ApplicationAge {model.ApplicationAge} must be between {vaccine.AgeStart} and {vaccine.AgeEnd} for Vaccine '{vaccineTemplate.VaccineName}'.");
                }

                vaccineTemplate.ApplicationAge = model.ApplicationAge.Value;
            }

            if (model.Session.HasValue)
            {
                vaccineTemplate.Session = model.Session.Value;
            }
            await _unitOfWork.VaccineTemplates.UpdateAsync(vaccineTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }


        public async Task<bool> DeleteVaccineTemplateAsync(Guid id)
        {
            
            var vaccineTemplate = await _unitOfWork.VaccineTemplates
                .FindByCondition(v => v.Id == id)
                .FirstOrDefaultAsync();

            if (vaccineTemplate == null)
            {
                return false;
            }
            await _unitOfWork.VaccineTemplates.DeleteAsync(vaccineTemplate);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<PagedResult<VaccineTemplateItemModel>> GetVaccineTemplatesAsync(VaccineTemplateFilterModel filter)
        {

            var query = _unitOfWork.VaccineTemplates.FindAll(false).AsQueryable();

            if (filter.TemplateId.HasValue)
            {
                query = query.Where(v => v.TemplateId == filter.TemplateId.Value);
            }

            if (!string.IsNullOrEmpty(filter.VaccineName))
            {
                query = query.Where(v => v.VaccineName.Contains(filter.VaccineName));
            }

            if (filter.Session.HasValue)
            {
                query = query.Where(v => v.Session == filter.Session.Value);
            }

            if (filter.ApplicationAge.HasValue)
            {
                query = query.Where(v => v.ApplicationAge == filter.ApplicationAge.Value);
            }

            var totalItems = await query.CountAsync();

            var items = await query
                .OrderBy(v => v.VaccineName)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(v => new VaccineTemplateItemModel
                {
                    Id = v.Id,
                    TemplateId = v.TemplateId,
                    VaccineName = v.VaccineName,
                    ApplicationMethod = v.ApplicationMethod,
                    ApplicationAge = v.ApplicationAge,
                    Session = v.Session
                })
                .ToListAsync();

            var result = new PaginatedList<VaccineTemplateItemModel>(items, totalItems,filter.PageNumber,filter.PageSize);
            return new PagedResult<VaccineTemplateItemModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage= result.HasNextPage,
                HasPreviousPage= result.HasPreviousPage
            };
        }

        public async Task<VaccineTemplateDetailModel?> GetVaccineTemplateDetailAsync(Guid id)
        {
            var vaccineTemplate = await _unitOfWork.VaccineTemplates
                .FindByCondition(v => v.Id == id)
                .Include(v => v.Template) 
                .FirstOrDefaultAsync();

            if (vaccineTemplate == null)
            {
                return null; 
            }

            return new VaccineTemplateDetailModel
            {
                Id = vaccineTemplate.Id,
                TemplateId = vaccineTemplate.TemplateId,
                VaccineName = vaccineTemplate.VaccineName,
                ApplicationMethod = vaccineTemplate.ApplicationMethod,
                ApplicationAge = (int)vaccineTemplate.ApplicationAge,
                Session = vaccineTemplate.Session,
                AnimalTemplate = vaccineTemplate.Template == null ? null : new AnimalTemplateResponse
                {
                    Id = vaccineTemplate.Template.Id,
                    Name = vaccineTemplate.Template.Name,
                    Species = vaccineTemplate.Template.Species,
                    Notes = vaccineTemplate.Template.Notes
                }
            };
        }





    }
}
