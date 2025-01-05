using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Symptom;
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

            await _unitOfWork.Symptoms.DeleteAsync(symptom);
            await _unitOfWork.CommitAsync();

            return true;
        }
    }
}
