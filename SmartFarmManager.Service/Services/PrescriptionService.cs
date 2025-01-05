using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Medication;
using SmartFarmManager.Service.BusinessModels.Prescription;
using SmartFarmManager.Service.BusinessModels.PrescriptionMedication;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Shared;
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
        private readonly IUserService _userService;

        public PrescriptionService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<Guid?> CreatePrescriptionAsync(PrescriptionModel model)
        {
            if (model.Status != PrescriptionStatusEnum.Active)
            {
                throw new ArgumentException($"Invalid status value: {model.Status}");
            }

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Lấy danh sách thuốc từ cơ sở dữ liệu
                var medicationIds = model.Medications.Select(m => m.MedicationId).Distinct();
                var medications = await _unitOfWork.Medication
                    .FindByCondition(m => medicationIds.Contains(m.Id))
                    .ToListAsync();

                if (medications == null || medications.Count == 0)
                    throw new Exception("One or more medications not found.");

                // Tính toán giá thuốc
                var totalPrice = model.Medications.Sum(m =>
                {
                    var medication = medications.FirstOrDefault(dbMed => dbMed.Id == m.MedicationId);
                    if (medication == null)
                        throw new Exception($"Medication with ID {m.MedicationId} not found.");

                    return (decimal)(m.Dosage * (medication.PricePerDose ?? 0));
                });

                // Tạo đơn thuốc
                var prescription = new Prescription
                {
                    RecordId = model.RecordId,
                    PrescribedDate = model.PrescribedDate,
                    Notes = model.Notes,
                    QuantityAnimal = model.QuantityAnimal,
                    Status = model.Status,
                    Price = totalPrice,
                    CageId = model.CageId,
                    DaysToTake = model.DaysToTake,
                    PrescriptionMedications = model.Medications.Select(m => new PrescriptionMedication
                    {
                        MedicationId = m.MedicationId,
                        Dosage = m.Dosage,
                        Morning = m.Morning,
                        Afternoon = m.Afternoon,
                        Evening = m.Evening,
                        Night = m.Night,
                    }).ToList()
                };

                await _unitOfWork.Prescription.CreateAsync(prescription);

                // Tạo danh sách TaskDaily và Task
                var taskDailyList = new List<TaskDaily>();
                var taskList = new List<DataAccessObject.Models.Task>();
                var taskType = await _unitOfWork.TaskTypes.FindByCondition(t => t.TaskTypeName == "Cho uống thuốc").FirstOrDefaultAsync();

                foreach (var medication in model.Medications)
                {
                    var sessionMappings = new List<(bool IsEnabled, SessionTypeEnum Session, string Time)>
            {
                (medication.Morning, SessionTypeEnum.Morning, "Buổi sáng"),
                (medication.Afternoon, SessionTypeEnum.Afternoon, "Buổi chiều"),
                (medication.Evening, SessionTypeEnum.Evening, "Buổi tối"),
                (medication.Night, SessionTypeEnum.Night, "Buổi đêm")
            };

                    foreach (var (isEnabled, sessionType, time) in sessionMappings)
                    {
                        if (!isEnabled) continue;

                        var lastDate = DateOnly.FromDateTime(model.PrescribedDate.AddDays((double)(model.DaysToTake - 1)));
                        var today = DateOnly.FromDateTime(DateTime.Now);
                        var tomorrow = today.AddDays(1);

                        // Tìm FarmingBatch với trạng thái "đang diễn ra"
                        var farmingBatch = await _unitOfWork.FarmingBatches.FindByCondition(
                            fb => fb.CageId == model.CageId && fb.Status == FarmingBatchStatusEnum.Active,
                            trackChanges: false
                        ).FirstOrDefaultAsync();

                        if (farmingBatch == null)
                            throw new Exception("FarmingBatch not found.");

                        // Tìm GrowthStage với trạng thái "đang diễn ra"
                        var growthStage = await _unitOfWork.GrowthStages.FindByCondition(
                            gs => gs.FarmingBatchId == farmingBatch.Id && gs.Status == GrowthStageStatusEnum.Active,
                            trackChanges: false
                        ).FirstOrDefaultAsync();

                        if (growthStage == null)
                            throw new Exception("GrowthStage not found.");

                        // Tạo TaskDaily nếu lịch kéo dài hơn hôm nay và mai
                        if (lastDate > tomorrow)
                        {
                            var taskDaily = new TaskDaily
                            {
                                GrowthStageId = growthStage.Id,
                                TaskTypeId = taskType.Id,
                                TaskName = $"Uống thuốc ({time})",
                                Description = $"Lịch uống thuốc từ {model.PrescribedDate:dd/MM/yyyy} đến {lastDate:dd/MM/yyyy} ({time})",
                                Session = (int)sessionType,
                                StartAt = model.PrescribedDate,
                                EndAt = model.PrescribedDate.AddDays((double)(model.DaysToTake - 1)),
                            };
                            taskDailyList.Add(taskDaily);
                        }

                        // Tạo Task cho hôm nay và mai
                        foreach (var date in new[] { today, tomorrow })
                        {
                            if (date <= lastDate)
                            {
                                // Gọi hàm để lấy User được phân công cho Cage
                                var assignedUserId = await _userService.GetAssignedUserForCageAsync(model.CageId, date);
                                if (assignedUserId == null)
                                {
                                    throw new Exception($"No user assigned to CageId {model.CageId} for date {date:dd/MM/yyyy}.");
                                }

                                var task = new DataAccessObject.Models.Task
                                {
                                    TaskTypeId = taskType.Id,
                                    CageId = model.CageId,
                                    AssignedToUserId = assignedUserId.Value,
                                    CreatedByUserId = null,
                                    TaskName = $"Uống thuốc ({time})",
                                    Description = $"Uống thuốc ngày {date:dd/MM/yyyy} ({time})",
                                    PriorityNum = 1,
                                    DueDate = date.ToDateTime(sessionType switch
                                    {
                                        SessionTypeEnum.Morning => new TimeOnly(8, 0),
                                        SessionTypeEnum.Afternoon => new TimeOnly(15, 0),
                                        SessionTypeEnum.Evening => new TimeOnly(18, 0),
                                        SessionTypeEnum.Night => new TimeOnly(21, 0),
                                        _ => TimeOnly.MaxValue
                                    }),
                                    Status = "Pending",
                                    Session = (int)sessionType,
                                    CreatedAt = DateTime.Now
                                };
                                taskList.Add(task);
                            }
                        }

                    }
                }

                // Lưu TaskDaily và Task
                if (taskDailyList.Any())
                {
                    await _unitOfWork.TaskDailies.CreateListAsync(taskDailyList);
                }
                if (taskList.Any())
                {
                    await _unitOfWork.Tasks.CreateListAsync(taskList);
                }

                await _unitOfWork.CommitAsync();
                return prescription.Id;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                Console.WriteLine($"Error in  create presciption: {ex.Message}");
                throw new Exception("Failed to create presciption. Details: " + ex.Message);
            }
        }



        public async Task<PrescriptionModel> GetPrescriptionByIdAsync(Guid id)
        {
            // Load the prescription along with PrescriptionMedications and their Medication
            var prescription = await _unitOfWork.Prescription
                .FindByCondition(p => p.Id == id)
                .Include(p => p.PrescriptionMedications)
                .ThenInclude(pm => pm.Medication)
                .FirstOrDefaultAsync();

            if (prescription == null)
                return null;

            // Map the data to the PrescriptionModel
            return new PrescriptionModel
            {
                Id = prescription.Id,
                RecordId = prescription.MedicalSymtomId,
                PrescribedDate = prescription.PrescribedDate.Value,
                Notes = prescription.Notes,
                QuantityAnimal = prescription.QuantityAnimal,
                Status = prescription.Status,
                Price = prescription.Price,
                CageId = prescription.CageId,
                DaysToTake = prescription.DaysToTake,
                Medications = prescription.PrescriptionMedications.Select(pm => new PrescriptionMedicationModel
                {
                    MedicationId = pm.MedicationId,
                    Dosage = pm.Dosage.Value,
                    Morning = pm.Morning,
                    Afternoon = pm.Afternoon,
                    Evening = pm.Evening,
                    Night = pm.Night,
                    Medication = new MedicationModel
                    {
                        Name = pm.Medication.Name,
                        UsageInstructions = pm.Medication.UsageInstructions,
                        Price = pm.Medication.Price,
                        DoseQuantity = pm.Medication.DoseQuantity
                    }
                }).ToList()
            };
        }


        public async Task<IEnumerable<PrescriptionModel>> GetActivePrescriptionsByCageIdAsync(Guid cageId)
        {
            var today = DateTime.UtcNow.Date;

            // Lấy danh sách đơn thuốc phù hợp
            var prescriptions = await _unitOfWork.Prescription
                .FindByCondition(p => p.CageId == cageId &&
                                      p.Status == "Đang sử dụng" &&
                                      p.PrescribedDate.HasValue &&
                                      p.DaysToTake.HasValue &&
                                      today >= p.PrescribedDate.Value.Date &&
                                      today <= p.PrescribedDate.Value.Date.AddDays(p.DaysToTake.Value),
                                      true, p => p.PrescriptionMedications, p => p.PrescriptionMedications.Select(pm => pm.Medication))
                .ToListAsync();

            // Trả về dữ liệu theo chuẩn
            return prescriptions.Select(p => new PrescriptionModel
            {
                Id = p.Id,
                CageId = p.CageId,
                PrescribedDate = p.PrescribedDate.Value,
                Notes = p.Notes,
                QuantityAnimal = p.QuantityAnimal,
                Status = p.Status,
                Price = p.Price,
                DaysToTake = p.DaysToTake,
                Medications = p.PrescriptionMedications.Select(pm => new PrescriptionMedicationModel
                {
                    MedicationId = pm.MedicationId,
                    Dosage = pm.Dosage.Value,
                    Morning = pm.Morning,
                    Afternoon = pm.Afternoon,
                    Evening = pm.Evening,
                    Night = pm.Night,
                    Medication = new MedicationModel
                    {
                        Name = pm.Medication.Name,
                        UsageInstructions = pm.Medication.UsageInstructions,
                        Price = pm.Medication.Price,
                        DoseQuantity = pm.Medication.DoseQuantity
                    }
                }).ToList()
            }).ToList();
        }

    }
}
