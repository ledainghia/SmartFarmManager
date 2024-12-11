using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Medication;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class MedicationService : IMedicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MedicationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MedicationModel> CreateMedicationAsync(MedicationModel medicationModel)
        {
            if (medicationModel == null)
            {
                throw new ArgumentNullException(nameof(medicationModel), "Medication model cannot be null.");
            }

            var medication = new Medication
            {
                Name = medicationModel.Name,
                UsageInstructions = medicationModel.UsageInstructions,
                Price = medicationModel.Price,
                DoseQuantity = medicationModel.DoseQuantity,
                PricePerDose = medicationModel.PricePerDose
            };

            await _unitOfWork.Medication.CreateAsync(medication);
            await _unitOfWork.CommitAsync();

            // Map back to MedicationModel to return
            return new MedicationModel
            {
                Id = medication.Id,
                Name = medication.Name,
                UsageInstructions = medication.UsageInstructions,
                Price = medication.Price,
                DoseQuantity = medication.DoseQuantity,
                PricePerDose = medication.PricePerDose
            };
        }


        public async Task<IEnumerable<MedicationModel>> GetAllMedicationsAsync()
        {
            var medication = await _unitOfWork.Medication.FindAll().ToListAsync();
            return _mapper.Map<List<MedicationModel>>(medication);
        }
    }

}
