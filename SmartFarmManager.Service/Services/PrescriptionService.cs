using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PrescriptionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Prescription> CreatePrescriptionAsync(Prescription prescription)
        {
            //await _unitOfWork.Prescription.CreateAsync(prescription);

            //foreach (var med in prescription.PrescriptionMedications)
            //{
            //    med.PrescriptionId = prescription.Id;
            //    await _unitOfWork.PrescriptionMedication.AddAsync(med);
            //}

            //await _unitOfWork.SaveAsync();
            return prescription;
        }
    }
}
