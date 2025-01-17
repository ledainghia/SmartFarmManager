using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Medication;
using SmartFarmManager.Service.BusinessModels.Prescription;
using SmartFarmManager.Service.BusinessModels.PrescriptionMedication;
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
                var medicalSymptom = await _unitOfWork.MedicalSymptom.FindByCondition(ms => ms.Id == model.RecordId.Value).FirstOrDefaultAsync();
                medicalSymptom.Status = MedicalSymptomStatuseEnum.Prescribed;

                // Tạo đơn thuốc
                var prescription = new Prescription
                {
                    MedicalSymtomId = model.RecordId.Value,
                    PrescribedDate = model.PrescribedDate,
                    Notes = model.Notes,
                    QuantityAnimal = model.QuantityAnimal.Value,
                    Status = model.Status,
                    CageId = model.CageId.Value,
                    DaysToTake = model.DaysToTake,
                    EndDate = model.PrescribedDate.Value.AddDays((double)model.DaysToTake),
                    PrescriptionMedications = model.Medications.Select(m => new PrescriptionMedication
                    {
                        MedicationId = m.MedicationId,
                        Morning = m.Morning,
                        Afternoon = m.Afternoon,
                        Evening = m.Evening,
                        Noon = m.Noon,
                    }).ToList()
                };

                await _unitOfWork.Prescription.CreateAsync(prescription);
                await _unitOfWork.MedicalSymptom.UpdateAsync(medicalSymptom);

                // Lấy thời gian hiện tại và buổi hiện tại
                var currentTime = DateTimeUtils.VietnamNow().TimeOfDay;
                var currentSession = SessionTime.GetCurrentSession(currentTime);

                // Kiểm tra đơn thuốc có thuốc kê cho các buổi sáng, trưa, chiều, tối hay không
                var hasMorningMedication = model.Medications.Any(m => m.Morning > 0);
                var hasNoonMedication = model.Medications.Any(m => m.Noon > 0);
                var hasAfternoonMedication = model.Medications.Any(m => m.Afternoon > 0);
                var hasEveningMedication = model.Medications.Any(m => m.Evening > 0);


                // Tạo danh sách TaskDaily và Task
                var taskList = new List<DataAccessObject.Models.Task>();
                var taskType = await _unitOfWork.TaskTypes.FindByCondition(t => t.TaskTypeName == "Cho uống thuốc").FirstOrDefaultAsync();

                // Tạo task cho ngày hiện tại
                DateOnly startDate = DateOnly.FromDateTime(DateTimeUtils.VietnamNow());
                TimeSpan startTime = TimeSpan.Zero;
                var assignedUserTodayId = await _userService.GetAssignedUserForCageAsync(model.CageId.Value, startDate);
                // Kiểm tra và tạo task cho buổi sáng
                if (currentSession <= 1 && hasMorningMedication) // Buổi sáng
                {
                    startTime = SessionTime.Morning.Start;
                    taskList.Add(new DataAccessObject.Models.Task
                    {
                        TaskTypeId = taskType.Id,
                        CageId = model.CageId.Value,
                        AssignedToUserId = assignedUserTodayId.Value, // Sẽ gán sau
                        CreatedByUserId = null,
                        TaskName = "Uống thuốc (Sáng)",
                        Description = $"Uống thuốc ngày {startDate:dd/MM/yyyy} (Sáng)",
                        PriorityNum = 1,
                        DueDate = startDate.ToDateTime(TimeOnly.MinValue),
                        Status = TaskStatusEnum.Pending,
                        Session = (int)SessionTypeEnum.Morning,
                        CreatedAt = DateTimeUtils.VietnamNow(),
                    });
                }

                // Kiểm tra và tạo task cho buổi trưa
                if (currentSession <= 2 && hasNoonMedication) // Buổi trưa
                {
                    startTime = SessionTime.Noon.Start;
                    taskList.Add(new DataAccessObject.Models.Task
                    {
                        TaskTypeId = taskType.Id,
                        CageId = model.CageId.Value,
                        AssignedToUserId = assignedUserTodayId.Value, // Sẽ gán sau
                        CreatedByUserId = null,
                        TaskName = "Uống thuốc (Trưa)",
                        Description = $"Uống thuốc ngày {startDate:dd/MM/yyyy} (Trưa)",
                        PriorityNum = 1,
                        DueDate = startDate.ToDateTime(TimeOnly.MinValue),
                        Status = TaskStatusEnum.Pending,
                        Session = (int)SessionTypeEnum.Noon,
                        CreatedAt = DateTimeUtils.VietnamNow(),
                    });
                }

                // Kiểm tra và tạo task cho buổi chiều
                if (currentSession <= 3 && hasAfternoonMedication) // Buổi chiều
                {
                    startTime = SessionTime.Afternoon.Start;
                    taskList.Add(new DataAccessObject.Models.Task
                    {
                        TaskTypeId = taskType.Id,
                        CageId = model.CageId.Value,
                        AssignedToUserId = assignedUserTodayId.Value, // Sẽ gán sau
                        CreatedByUserId = null,
                        TaskName = "Uống thuốc (Chiều)",
                        Description = $"Uống thuốc ngày {startDate:dd/MM/yyyy} (Chiều)",
                        PriorityNum = 1,
                        DueDate = startDate.ToDateTime(TimeOnly.MinValue),
                        Status = TaskStatusEnum.Pending,
                        Session = (int)SessionTypeEnum.Afternoon,
                        CreatedAt = DateTimeUtils.VietnamNow(),
                    });
                }

                // Kiểm tra và tạo task cho buổi tối
                if (currentSession <= 4 && hasEveningMedication) // Buổi tối
                {
                    startTime = SessionTime.Evening.Start;
                    taskList.Add(new DataAccessObject.Models.Task
                    {
                        TaskTypeId = taskType.Id,
                        CageId = model.CageId.Value,
                        AssignedToUserId = assignedUserTodayId.Value, // Sẽ gán sau
                        CreatedByUserId = null,
                        TaskName = "Uống thuốc (Tối)",
                        Description = $"Uống thuốc ngày {startDate:dd/MM/yyyy} (Tối)",
                        PriorityNum = 1,
                        DueDate = startDate.ToDateTime(TimeOnly.MinValue),
                        Status = TaskStatusEnum.Pending,
                        Session = (int)SessionTypeEnum.Evening,
                        CreatedAt = DateTimeUtils.VietnamNow(),
                    });
                }

                var lastDate = startDate.AddDays((model.DaysToTake.Value - 1));
                

                // Tạo task cho ngày mai nếu có thuốc kê cho buổi sáng, trưa, chiều, tối
                var tomorrow = startDate.AddDays(1);

                // Kiểm tra có thuốc kê cho buổi sáng, trưa, chiều, tối ngày mai
                if (tomorrow <= lastDate)
                {
                    // Kiểm tra và tạo task cho buổi sáng ngày mai nếu có thuốc kê cho sáng
                    if (hasMorningMedication)
                    {
                        var assignedUserId = await _userService.GetAssignedUserForCageAsync(model.CageId.Value, tomorrow);
                        if (assignedUserId != null)
                        {
                            taskList.Add(new DataAccessObject.Models.Task
                            {
                                TaskTypeId = taskType.Id,
                                CageId = model.CageId.Value,
                                AssignedToUserId = assignedUserId.Value,
                                CreatedByUserId = null,
                                TaskName = $"Uống thuốc",
                                Description = $"Uống thuốc ngày {tomorrow:dd/MM/yyyy} (Sáng)",
                                PriorityNum = taskType.PriorityNum.Value,
                                DueDate = tomorrow.ToDateTime(TimeOnly.MinValue),
                                Status = TaskStatusEnum.Pending,
                                Session = (int)SessionTypeEnum.Morning,
                                CreatedAt = DateTimeUtils.VietnamNow(),
                            });
                        }
                    }

                    // Tạo task cho buổi trưa ngày mai nếu có thuốc kê cho trưa
                    if (hasNoonMedication)
                    {
                        var assignedUserId = await _userService.GetAssignedUserForCageAsync(model.CageId.Value, tomorrow);
                        if (assignedUserId != null)
                        {
                            taskList.Add(new DataAccessObject.Models.Task
                            {
                                TaskTypeId = taskType.Id,
                                CageId = model.CageId.Value,
                                AssignedToUserId = assignedUserId.Value,
                                CreatedByUserId = null,
                                TaskName = $"Uống thuốc",
                                Description = $"Uống thuốc ngày {tomorrow:dd/MM/yyyy} (Trưa)",
                                PriorityNum = taskType.PriorityNum.Value,
                                DueDate = tomorrow.ToDateTime(TimeOnly.MinValue),
                                Status = TaskStatusEnum.Pending,
                                Session = (int)SessionTypeEnum.Noon,
                                CreatedAt = DateTimeUtils.VietnamNow(),
                            });
                        }
                    }

                    // Tạo task cho buổi chiều ngày mai nếu có thuốc kê cho chiều
                    if (hasAfternoonMedication)
                    {
                        var assignedUserId = await _userService.GetAssignedUserForCageAsync(model.CageId.Value, tomorrow);
                        if (assignedUserId != null)
                        {
                            taskList.Add(new DataAccessObject.Models.Task
                            {
                                TaskTypeId = taskType.Id,
                                CageId = model.CageId.Value,
                                AssignedToUserId = assignedUserId.Value,
                                CreatedByUserId = null,
                                TaskName = $"Uống thuốc",
                                Description = $"Uống thuốc ngày {tomorrow:dd/MM/yyyy} (Chiều)",
                                PriorityNum = taskType.PriorityNum.Value,
                                DueDate = tomorrow.ToDateTime(TimeOnly.MinValue),
                                Status = TaskStatusEnum.Pending,
                                Session = (int)SessionTypeEnum.Afternoon,
                                CreatedAt = DateTimeUtils.VietnamNow(),
                            });
                        }
                    }

                    // Tạo task cho buổi tối ngày mai nếu có thuốc kê cho tối
                    if (hasEveningMedication)
                    {
                        var assignedUserId = await _userService.GetAssignedUserForCageAsync(model.CageId.Value, tomorrow);
                        if (assignedUserId != null)
                        {
                            taskList.Add(new DataAccessObject.Models.Task
                            {
                                TaskTypeId = taskType.Id,
                                CageId = model.CageId.Value,
                                AssignedToUserId = assignedUserId.Value,
                                CreatedByUserId = null,
                                TaskName = $"Uống thuốc",
                                Description = $"Uống thuốc ngày {tomorrow:dd/MM/yyyy} (Tối)",
                                PriorityNum = taskType.PriorityNum.Value,
                                DueDate = tomorrow.ToDateTime(TimeOnly.MinValue),
                                Status = TaskStatusEnum.Pending,
                                Session = (int)SessionTypeEnum.Evening,
                                CreatedAt = DateTimeUtils.VietnamNow(),
                            });
                        }
                    }
                }

                // Lưu TaskDaily và Task
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
                Console.WriteLine($"Error in create prescription: {ex.Message}");
                throw new Exception("Failed to create prescription. Details: " + ex.Message);
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
                    Morning = pm.Morning,
                    Afternoon = pm.Afternoon,
                    Evening = pm.Evening,
                    Noon = pm.Noon,
                    Notes = pm.Notes,
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
                    Morning = pm.Morning,
                    Afternoon = pm.Afternoon,
                    Evening = pm.Evening,
                    Noon = pm.Noon,
                    Notes = pm.Notes,
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
        public async Task<bool> UpdatePrescriptionAsync(PrescriptionModel model)
        {
            var prescription = await _unitOfWork.Prescription.FindByCondition(m => m.Id == model.Id).FirstOrDefaultAsync();

            if (prescription == null)
                return false;

            // Chỉ cập nhật các trường có giá trị
            if (model.PrescribedDate.HasValue)
                prescription.PrescribedDate = model.PrescribedDate;

            if (!string.IsNullOrWhiteSpace(model.Notes))
                prescription.Notes = model.Notes;

            if (model.QuantityAnimal.HasValue)
                prescription.QuantityAnimal = model.QuantityAnimal.Value;

            if (!string.IsNullOrWhiteSpace(model.Status))
                prescription.Status = model.Status;

            if (model.Price.HasValue)
                prescription.Price = model.Price.Value;

            if (model.CageId.HasValue)
                prescription.CageId = model.CageId.Value;

            if (model.DaysToTake.HasValue)
                prescription.DaysToTake = model.DaysToTake.Value;

            // Lưu thay đổi
            await _unitOfWork.Prescription.UpdateAsync(prescription);
            await _unitOfWork.CommitAsync();

            return true;
        }

    }
}
