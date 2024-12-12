﻿using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.MedicalSymptom;
using SmartFarmManager.Service.BusinessModels.Picture;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class MedicalSymptomService : IMedicalSymptomService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MedicalSymptomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MedicalSymptomModel>> GetMedicalSymptomsAsync(string? status)
        {
            var symptoms = await _unitOfWork.MedicalSymptom
                .FindAll().Where(ms => string.IsNullOrEmpty(status) || ms.Status == status).Include(p => p.Pictures).ToListAsync();

            return symptoms.Select(ms => new MedicalSymptomModel
            {
                Id = ms.Id,
                FarmingBatchId = ms.FarmingBatchId,
                Symptoms = ms.Symptoms,
                Diagnosis = ms.Diagnosis,
                Treatment = ms.Treatment,
                Status = ms.Status,
                AffectedQuantity = ms.AffectedQuantity,
                Notes = ms.Notes,
                Pictures = ms.Pictures.Select(p => new PictureModel
                {
                    Id = p.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList()
            });
        }
        public async Task<bool> UpdateMedicalSymptomAsync(MedicalSymptomModel updatedModel)
        {
            var existingSymptom = await _unitOfWork.MedicalSymptom
                .GetByIdAsync(updatedModel.Id);

            if (existingSymptom == null)
            {
                return false;
            }

            // Cập nhật thông tin
            existingSymptom.Diagnosis = updatedModel.Diagnosis;
            existingSymptom.Treatment = updatedModel.Treatment;
            existingSymptom.Status = updatedModel.Status;
            existingSymptom.Notes = updatedModel.Notes;

            await _unitOfWork.MedicalSymptom.UpdateAsync(existingSymptom);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<Guid> CreateMedicalSymptomAsync(MedicalSymptomModel medicalSymptomModel)
        {
            var medicalSymptom = new DataAccessObject.Models.MedicalSymptom
            {
                FarmingBatchId = medicalSymptomModel.FarmingBatchId,
                Symptoms = medicalSymptomModel.Symptoms,
                Status = medicalSymptomModel.Status,
                AffectedQuantity = medicalSymptomModel.AffectedQuantity,
                Notes = medicalSymptomModel.Notes,
                Pictures = medicalSymptomModel.Pictures.Select(p => new DataAccessObject.Models.Picture
                {
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList()
            };

            await _unitOfWork.MedicalSymptom.CreateAsync(medicalSymptom);
            await _unitOfWork.CommitAsync();

            return medicalSymptom.Id;
        }

        public async Task<MedicalSymptomModel?> GetMedicalSymptomByIdAsync(Guid id)
        {
            var medicalSymptom = await _unitOfWork.MedicalSymptom.FindAll()
                .Where(m => m.Id == id).Include(p => p.Pictures).FirstOrDefaultAsync();

            if (medicalSymptom == null)
            {
                return null;
            }

            return new MedicalSymptomModel
            {
                Id = medicalSymptom.Id,
                FarmingBatchId = medicalSymptom.FarmingBatchId,
                Symptoms = medicalSymptom.Symptoms,
                Diagnosis = medicalSymptom.Diagnosis,
                Treatment = medicalSymptom.Treatment,
                Status = medicalSymptom.Status,
                AffectedQuantity = medicalSymptom.AffectedQuantity,
                Notes = medicalSymptom.Notes,
                Pictures = medicalSymptom.Pictures.Select(p => new PictureModel
                {
                    Id = p.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList()
            };
        }
    }
}