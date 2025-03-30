using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.Symptom;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class SymptomService : ISymptomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SymptomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<SymptomModel>> GetAllSymptomsAsync()
        {
            var symptoms = await _unitOfWork.Symptoms.FindAll().ToListAsync();
            return symptoms.Select(s => new SymptomModel
            {
                Id = s.Id,
                SymptomName = s.SymptomName
            }).ToList();
        }

        public async Task<SymptomModel> GetSymptomByIdAsync(Guid id)
        {
            var symptom = await _unitOfWork.Symptoms.GetByIdAsync(id);
            if (symptom == null)
            {
                return null;
            }

            return new SymptomModel
            {
                Id = symptom.Id,
                SymptomName = symptom.SymptomName
            };
        }

        public async Task<Guid> CreateSymptomAsync(SymptomModel symptomModel)
        {
            var symptomExist =  _unitOfWork.Symptoms.FindByCondition(s => s.SymptomName.Equals(symptomModel.SymptomName) && s.IsDeleted == false);
            if(symptomExist != null)
            {
                throw new ArgumentException($" Symptom with name '{symptomModel.SymptomName}' already exists.");
            }
            var symptom = new DataAccessObject.Models.Symptom
            {
                SymptomName = symptomModel.SymptomName
            };

            await _unitOfWork.Symptoms.CreateAsync(symptom);
            await _unitOfWork.CommitAsync();

            return symptom.Id;
        }

        public async Task<bool> UpdateSymptomAsync(SymptomModel symptomModel)
        {
            var symptom = await _unitOfWork.Symptoms.GetByIdAsync(symptomModel.Id.Value);
            if (symptom == null)
            {
                return false;
            }

            symptom.SymptomName = symptomModel.SymptomName;
            await _unitOfWork.Symptoms.UpdateAsync(symptom);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<bool> DeleteSymptomAsync(Guid id)
        {
            var symptom = await _unitOfWork.Symptoms.GetByIdAsync(id);
            if (symptom == null)
            {
                return false;
            }
            symptom.IsDeleted = true;

            await _unitOfWork.Symptoms.UpdateAsync(symptom);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<PagedResult<SymptomModel>> GetSymptomsAsync(SymptomFilterModel filter)
        {
            var query = _unitOfWork.Symptoms
                .FindAll(false)
                .AsQueryable();

            // Tìm kiếm theo tên triệu chứng nếu có
            if (!string.IsNullOrEmpty(filter.KeySearch))
            {
                query = query.Where(s => s.SymptomName.Contains(filter.KeySearch));
            }
            if (filter.IsDeleted.HasValue)
            {
                query = query.Where(s => s.IsDeleted == filter.IsDeleted);
            }

            var totalItems = await query.CountAsync();

            var symptoms = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(s => new SymptomModel
                {
                    Id = s.Id,
                    SymptomName = s.SymptomName,
                    IsDeleted = s.IsDeleted
                })
                .ToListAsync();

            var result = new PaginatedList<SymptomModel>(symptoms, totalItems, filter.PageNumber, filter.PageSize);

            return new PagedResult<SymptomModel>
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

    }
}
