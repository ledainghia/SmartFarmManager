using Microsoft.EntityFrameworkCore;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.MedicalSymptom;
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
                var currentTime = DateTimeUtils.GetServerTimeInVietnamTime().TimeOfDay;
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
                DateOnly startDate = DateOnly.FromDateTime(DateTimeUtils.GetServerTimeInVietnamTime());
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
                        CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
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
                        CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
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
                        CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
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
                        CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
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
                                CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
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
                                CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
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
                                CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
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
                                CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
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
                .Include(p => p.MedicalSymtom).ThenInclude(ms => ms.MedicalSymptomDetails).ThenInclude(msd => msd.Symptom)
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
                Symptoms = string.Join(", ", prescription.MedicalSymtom?.MedicalSymptomDetails?.Where(d => d.Symptom != null)
                            .Select(d => d.Symptom.SymptomName) ?? new List<string>()),
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

        public async Task<bool> IsLastPrescriptionSessionAsync(Guid prescriptionId)
        {
            //// 🔹 Tìm đơn thuốc theo ID và lấy luôn danh sách thuốc trong đơn
            //var prescription = await _unitOfWork.Prescription
            //    .FindByCondition(p => p.Id == prescriptionId)
            //    .Include(p => p.PrescriptionMedications)
            //    .FirstOrDefaultAsync();
            var prescription = await _unitOfWork.Prescription
                .FindByCondition(p => p.Id == prescriptionId && p.Status == PrescriptionStatusEnum.Active)
                .Include(p => p.PrescriptionMedications)
                .Include(p => p.MedicalSymtom)
                .ThenInclude(p => p.FarmingBatch)
                .FirstOrDefaultAsync();

            // ✅ Kiểm tra nếu đơn thuốc không tồn tại
            if (prescription == null || !prescription.EndDate.HasValue)
                return false;

            // 🔹 Kiểm tra xem có thuốc kê vào từng buổi hay không
            var hasMorningMedication = prescription.PrescriptionMedications.Any(m => m.Morning > 0);
            var hasNoonMedication = prescription.PrescriptionMedications.Any(m => m.Noon > 0);
            var hasAfternoonMedication = prescription.PrescriptionMedications.Any(m => m.Afternoon > 0);
            var hasEveningMedication = prescription.PrescriptionMedications.Any(m => m.Evening > 0);

            // 🔹 Lấy thời gian hiện tại theo giờ server (Việt Nam)
            var now = DateTimeUtils.GetServerTimeInVietnamTime();
            var currentTime = now.TimeOfDay;
            var currentSession = SessionTime.GetCurrentSession(currentTime);

            // ✅ Nếu hôm nay không phải ngày cuối → return false
            if (now.Date != prescription.EndDate.Value.Date)
                return false;

            // ✅ Kiểm tra xem có phải buổi cuối cùng không
            var isLastSession = currentSession switch
            {
                1 => !hasNoonMedication && !hasAfternoonMedication && !hasEveningMedication,  // Morning là buổi cuối
                2 => !hasAfternoonMedication && !hasEveningMedication,                        // Noon là buổi cuối
                3 => !hasEveningMedication,                                                  // Afternoon là buổi cuối
                4 => true,                                                                   // Evening là buổi cuối
                _ => false
            };
            if (isLastSession)
            {
                var checkListPrescription = await _unitOfWork.Prescription.FindByCondition(p => p.MedicalSymtomId == prescription.MedicalSymtomId && p.Status == PrescriptionStatusEnum.Active).CountAsync();
                if (checkListPrescription > 1)
                {
                    return false;
                }
            }
            return true;
        }
        public async Task<bool> UpdatePrescriptionStatusAsync(Guid prescriptionId, UpdatePrescriptionModel request)
        {
            // 🔹 Lấy đơn thuốc từ DB
            var prescription = await _unitOfWork.Prescription
                .FindByCondition(p => p.Id == prescriptionId && p.Status == PrescriptionStatusEnum.Active)
                .Include(p => p.PrescriptionMedications)
                .Include(p => p.MedicalSymtom)
                .ThenInclude(ms => ms.FarmingBatch)
                .FirstOrDefaultAsync();

            // ❌ Kiểm tra nếu đơn thuốc không tồn tại
            if (prescription == null)
                throw new ArgumentException("Prescription not found or not active.");

            // ❌ Kiểm tra nếu trạng thái không hợp lệ
            if (request.Status != PrescriptionStatusEnum.Completed && request.Status != PrescriptionStatusEnum.Stop)
                throw new ArgumentException("Invalid status. Only 'Completed' or 'Dead' are allowed.");

            // ✅ Kiểm tra số lượng vật nuôi bị ảnh hưởng
            if (request.Status == PrescriptionStatusEnum.Completed)
            {
                if (request.RemainingQuantity == null)
                    throw new ArgumentException("RemainingQuantity is required for status 'Completed'.");

                if (request.RemainingQuantity > prescription.QuantityAnimal)
                    throw new ArgumentException("Remaining quantity cannot exceed total affected animals.");

                prescription.RemainingQuantity = request.RemainingQuantity;
            }
            else if (request.Status == PrescriptionStatusEnum.Dead)
            {
                prescription.RemainingQuantity = 0; // ✅ Nếu chết hết, RemainingQuantity = 0
            }

            // ✅ Cập nhật trạng thái đơn thuốc
            prescription.Status = request.Status;

            // 🔹 Cập nhật số lượng bị ảnh hưởng trong **FarmingBatch**
            var farmingBatch = prescription.MedicalSymtom?.FarmingBatch;
            if (farmingBatch != null)
            {
                farmingBatch.AffectedQuantity -= prescription.RemainingQuantity ?? 0;
                await _unitOfWork.FarmingBatches.UpdateAsync(farmingBatch);
            }

            // ✅ Lưu thay đổi
            await _unitOfWork.Prescription.UpdateAsync(prescription);
            await _unitOfWork.CommitAsync();

            return true;
        }
        public async Task<bool> CreateNewPrescriptionAsync(UpdateMedicalSymptomModel request)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var serverTime = DateTimeUtils.GetServerTimeInVietnamTime();
                var currentSessionCheck = SessionTime.GetCurrentSession(serverTime.TimeOfDay);

                // 🔹 Lấy triệu chứng đang điều trị
                var medicalSymptom = await _unitOfWork.MedicalSymptom
                    .FindByCondition(ms => ms.Id == request.Id)
                    .Include(ms => ms.Prescriptions)
                    .ThenInclude(p => p.PrescriptionMedications)
                    .FirstOrDefaultAsync();

                if (medicalSymptom == null)
                    return false;

                // 🔹 Lấy đơn thuốc Active hiện tại
                var activePrescription = medicalSymptom.Prescriptions
                    .Where(p => p.Status == PrescriptionStatusEnum.Active)
                    .OrderByDescending(p => p.PrescribedDate)
                    .FirstOrDefault();

                if (activePrescription != null)
                {
                    // 🔹 Tính lại tổng số liều đã uống
                    int totalDosesTaken = 0;
                    decimal totalCost = 0;

                    foreach (var pm in activePrescription.PrescriptionMedications)
                    {
                        int takenMorning = 0, takenNoon = 0, takenAfternoon = 0, takenEvening = 0;

                        // ✅ Tính số liều đã uống theo từng session
                        if (serverTime.Date > activePrescription.PrescribedDate.Value.Date)
                        {
                            // Đã qua ít nhất 1 ngày
                            int fullDaysPassed = (serverTime.Date - activePrescription.PrescribedDate.Value.Date).Days;
                            takenMorning = pm.Morning * fullDaysPassed;
                            takenNoon = pm.Noon * fullDaysPassed;
                            takenAfternoon = pm.Afternoon * fullDaysPassed;
                            takenEvening = pm.Evening * fullDaysPassed;
                        }

                        // ✅ Tính số liều trong ngày hiện tại
                        if (serverTime.Date == activePrescription.PrescribedDate.Value.Date || currentSessionCheck > 0)
                        {
                            if (currentSessionCheck >= 1) takenMorning += pm.Morning;
                            if (currentSessionCheck >= 2) takenNoon += pm.Noon;
                            if (currentSessionCheck >= 3) takenAfternoon += pm.Afternoon;
                            if (currentSessionCheck >= 4) takenEvening += pm.Evening;
                        }

                        int totalDosesForMedication = takenMorning + takenNoon + takenAfternoon + takenEvening;
                        totalDosesTaken += totalDosesForMedication;

                        // ✅ Tính tổng tiền
                        var medication = await _unitOfWork.Medication.GetByIdAsync(pm.MedicationId);
                        if (medication?.PricePerDose != null)
                        {
                            totalCost += totalDosesForMedication * medication.PricePerDose.Value;
                        }
                    }

                    // 🔹 Cập nhật giá trị cho đơn thuốc cũ
                    activePrescription.Status = PrescriptionStatusEnum.Stop;
                    activePrescription.EndDate = serverTime;
                    activePrescription.Price = totalCost;
                    activePrescription.RemainingQuantity = request.Prescriptions.QuantityAnimal;
                    await _unitOfWork.Prescription.UpdateAsync(activePrescription);
                }
                var tasksToUpdate = await _unitOfWork.Tasks
                .FindByCondition(t =>
                    t.PrescriptionId == activePrescription.Id &&
                    t.DueDate >= serverTime.Date &&
                    t.Session > currentSessionCheck &&
                    t.Status == TaskStatusEnum.Pending || t.Status == TaskStatusEnum.InProgress)
                .ToListAsync();

                foreach (var task in tasksToUpdate)
                {
                    task.Status = TaskStatusEnum.Done;
                }

                await _unitOfWork.Tasks.UpdateListAsync(tasksToUpdate);
                var cage = await _unitOfWork.Cages.FindByCondition(c => c.IsDeleted == false && c.IsSolationCage == true).FirstOrDefaultAsync();
                Guid? newPrescriptionId = null;
                // 🔹 Tạo đơn thuốc mới
                var medications = request.Prescriptions.Medications;

                var totalPrice = medications.Sum(m =>
                {
                    var medication = _unitOfWork.Medication.GetByIdAsync(m.MedicationId).Result; // Lấy thông tin thuốc

                    // Tính tổng số liều (Morning + Noon + Afternoon + Evening)
                    var totalDoses = m.Morning + m.Noon + m.Afternoon + m.Evening;

                    // Tính giá dựa trên tổng số liều và giá mỗi liều
                    return medication.PricePerDose.HasValue ? medication.PricePerDose.Value * totalDoses : 0;
                });
                // Kiểm tra trạng thái đơn thuốc có hợp lệ không
                if (request.Status != PrescriptionStatusEnum.Active)
                {
                    throw new ArgumentException($"Trạng thái đơn thuốc không hợp lệ: {request.Status}");
                }
                var newPrescription = new Prescription
                {
                    MedicalSymtomId = request.Id,
                    CageId = cage.Id,
                    //PrescribedDate = updatedModel.Prescriptions.PrescribedDate,
                    PrescribedDate = DateTimeUtils.GetServerTimeInVietnamTime(),
                    Notes = request.Notes,
                    DaysToTake = request.Prescriptions.DaysToTake,
                    Status = request.Status,
                    QuantityAnimal = request.Prescriptions.QuantityAnimal.Value,
                    //EndDate = updatedModel.Prescriptions.PrescribedDate.Value.AddDays((double)updatedModel.Prescriptions.DaysToTake),
                    EndDate = DateTimeUtils.GetServerTimeInVietnamTime().AddDays((double)request.Prescriptions.DaysToTake),
                    Price = totalPrice * request.Prescriptions.DaysToTake * request.Prescriptions.QuantityAnimal.Value
                };

                await _unitOfWork.Prescription.CreateAsync(newPrescription);
                var newPrescriptionMedication = request.Prescriptions.Medications.Select(m => new PrescriptionMedication
                {
                    PrescriptionId = newPrescription.Id,
                    Notes = m.Notes,
                    MedicationId = m.MedicationId,
                    Morning = m.Morning,
                    Afternoon = m.Afternoon,
                    Evening = m.Evening,
                    Noon = m.Noon
                }).ToList();
                await _unitOfWork.PrescriptionMedications.CreateListAsync(newPrescriptionMedication);
                newPrescriptionId = newPrescription.Id;
                //update affectedQuantity in farmingBatch
                var symtom = await _unitOfWork.MedicalSymptom.FindByCondition(ms => ms.Id == request.Id).Include(ms => ms.FarmingBatch).FirstOrDefaultAsync();
                var farmingBatch = await _unitOfWork.FarmingBatches.FindByCondition(c => c.Id == symtom.FarmingBatch.Id).FirstOrDefaultAsync();
                farmingBatch.AffectedQuantity += request.Prescriptions.QuantityAnimal.Value;
                await _unitOfWork.FarmingBatches.UpdateAsync(farmingBatch);

                

                //create task in today and tomorow
                // Lấy thời gian hiện tại và buổi hiện tại
                var currentTime = DateTimeUtils.GetServerTimeInVietnamTime().TimeOfDay;
                var currentSession = SessionTime.GetCurrentSession(currentTime);

                // Kiểm tra đơn thuốc có thuốc kê cho các buổi sáng, trưa, chiều, tối hay không
                var hasMorningMedication = request.Prescriptions.Medications.Any(m => m.Morning > 0);
                var hasNoonMedication = request.Prescriptions.Medications.Any(m => m.Noon > 0);
                var hasAfternoonMedication = request.Prescriptions.Medications.Any(m => m.Afternoon > 0);
                var hasEveningMedication = request.Prescriptions.Medications.Any(m => m.Evening > 0);

                // Tạo danh sách TaskDaily và Task
                var taskList = new List<DataAccessObject.Models.Task>();
                var taskType = await _unitOfWork.TaskTypes.FindByCondition(t => t.TaskTypeName == "Cho uống thuốc").FirstOrDefaultAsync();

                // Tạo task cho ngày hiện tại
                DateOnly startDate = DateOnly.FromDateTime(DateTimeUtils.GetServerTimeInVietnamTime());
                TimeSpan startTime = TimeSpan.Zero;
                var assignedUserTodayId = await _userService.GetAssignedUserForCageAsync(cage.Id, startDate);

                var medicationIds = request.Prescriptions.Medications.Select(m => m.MedicationId).ToList();

                // Truy vấn từ cơ sở dữ liệu để lấy MedicationName dựa trên MedicationId
                var medicationList = await _unitOfWork.Medication
                    .FindByCondition(m => medicationIds.Contains(m.Id))
                    .Select(m => new { m.Id, m.Name })
                    .ToListAsync();

                // Bước 1: Lấy danh sách MedicationId
                var medicationListIds = newPrescriptionMedication.Select(pm => pm.MedicationId).ToList();

                // Bước 2: Truy vấn cơ sở dữ liệu để lấy MedicationName
                var medicationsList = await _unitOfWork.Medication
                    .FindByCondition(m => medicationIds.Contains(m.Id))
                    .ToListAsync();
                var prescriptionMedicationsWithNames = newPrescriptionMedication.Select(pm => new
                {
                    pm.MedicationId,
                    MedicationName = medicationsList.FirstOrDefault(m => m.Id == pm.MedicationId)?.Name,
                    pm.Notes,
                    pm.Morning,
                    pm.Noon,
                    pm.Afternoon,
                    pm.Evening
                }).ToList();

                var sessionTasks = new Dictionary<int, List<(string MedicationName, int Quantity)>>();

                sessionTasks[(int)SessionTypeEnum.Morning] = prescriptionMedicationsWithNames
                    .Where(pm => pm.Morning > 0)
                    .Select(pm => (pm.MedicationName, pm.Morning))
                    .ToList();

                sessionTasks[(int)SessionTypeEnum.Noon] = prescriptionMedicationsWithNames
                    .Where(pm => pm.Noon > 0)
                    .Select(pm => (pm.MedicationName, pm.Noon))
                    .ToList();

                sessionTasks[(int)SessionTypeEnum.Afternoon] = prescriptionMedicationsWithNames
                    .Where(pm => pm.Afternoon > 0)
                    .Select(pm => (pm.MedicationName, pm.Afternoon))
                    .ToList();

                sessionTasks[(int)SessionTypeEnum.Evening] = prescriptionMedicationsWithNames
                    .Where(pm => pm.Evening > 0)
                    .Select(pm => (pm.MedicationName, pm.Evening))
                    .ToList();

                // Kiểm tra và tạo task cho buổi sáng
                if (currentSession < 1 && currentSession > 0 && hasMorningMedication) // Buổi sáng
                {
                    var morningMedications = sessionTasks[(int)SessionTypeEnum.Morning];
                    var medicationDetails = string.Join(", ", morningMedications.Select(m => $"{m.MedicationName} (Số liều: {m.Quantity})"));

                    startTime = SessionTime.Morning.Start;
                    taskList.Add(new DataAccessObject.Models.Task
                    {

                        TaskTypeId = taskType.Id,
                        CageId = cage.Id,
                        AssignedToUserId = assignedUserTodayId.Value, // Sẽ gán sau
                        CreatedByUserId = null,
                        TaskName = "Uống thuốc (Sáng)",
                        Description = $"Điều trị cho {newPrescription.QuantityAnimal} con. Thuốc: {medicationDetails}.",
                        PriorityNum = 1,
                        DueDate = startDate.ToDateTime(TimeOnly.MinValue),
                        //Status = currentSession == 1 ? TaskStatusEnum.InProgress : TaskStatusEnum.Pending,
                        Status = TaskStatusEnum.Pending,
                        Session = (int)SessionTypeEnum.Morning,
                        CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                        PrescriptionId = newPrescription.Id,
                        IsTreatmentTask = true
                    });
                }

                // Kiểm tra và tạo task cho buổi trưa
                if (currentSession < 2 && currentSession > 0 && hasNoonMedication) // Buổi trưa
                {
                    var noonMedications = sessionTasks[(int)SessionTypeEnum.Noon];
                    var medicationDetails = string.Join(", ", noonMedications.Select(m => $"{m.MedicationName} (Số liều: {m.Quantity})"));
                    startTime = SessionTime.Noon.Start;
                    taskList.Add(new DataAccessObject.Models.Task
                    {
                        TaskTypeId = taskType.Id,
                        CageId = cage.Id,
                        AssignedToUserId = assignedUserTodayId.Value, // Sẽ gán sau
                        CreatedByUserId = null,
                        TaskName = "Uống thuốc (Trưa)",
                        Description = $"Điều trị cho {newPrescription.QuantityAnimal} con. Thuốc: {medicationDetails}.",
                        PriorityNum = 1,
                        DueDate = startDate.ToDateTime(TimeOnly.MinValue),
                        Status = TaskStatusEnum.Pending,
                        Session = (int)SessionTypeEnum.Noon,
                        CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                        PrescriptionId = newPrescription.Id,
                        IsTreatmentTask = true
                    });
                }

                // Kiểm tra và tạo task cho buổi chiều
                if (currentSession < 3 && currentSession > 0 && hasAfternoonMedication) // Buổi chiều
                {
                    var afternoonMedications = sessionTasks[(int)SessionTypeEnum.Afternoon];
                    var medicationDetails = string.Join(", ", afternoonMedications.Select(m => $"{m.MedicationName} (Số liều: {m.Quantity})"));
                    startTime = SessionTime.Afternoon.Start;
                    taskList.Add(new DataAccessObject.Models.Task
                    {
                        TaskTypeId = taskType.Id,
                        CageId = cage.Id,
                        AssignedToUserId = assignedUserTodayId.Value, // Sẽ gán sau
                        CreatedByUserId = null,
                        TaskName = "Uống thuốc (Chiều)",
                        Description = $"Điều trị cho {newPrescription.QuantityAnimal} con. Thuốc: {medicationDetails}.",
                        PriorityNum = 1,
                        DueDate = startDate.ToDateTime(TimeOnly.MinValue),
                        Status = TaskStatusEnum.Pending,
                        Session = (int)SessionTypeEnum.Afternoon,
                        CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                        PrescriptionId = newPrescription.Id,
                        IsTreatmentTask = true
                    });
                }

                // Kiểm tra và tạo task cho buổi tối
                if (currentSession < 4 && currentSession > 0 && hasEveningMedication) // Buổi tối
                {
                    var eveningMedications = sessionTasks[(int)SessionTypeEnum.Evening];
                    var medicationDetails = string.Join(", ", eveningMedications.Select(m => $"{m.MedicationName} (Số liều: {m.Quantity})"));
                    startTime = SessionTime.Evening.Start;
                    taskList.Add(new DataAccessObject.Models.Task
                    {
                        TaskTypeId = taskType.Id,
                        CageId = cage.Id,
                        AssignedToUserId = assignedUserTodayId.Value, // Sẽ gán sau
                        CreatedByUserId = null,
                        TaskName = "Uống thuốc (Tối)",
                        Description = $"Điều trị cho {newPrescription.QuantityAnimal} con. Thuốc: {medicationDetails}.",
                        PriorityNum = 1,
                        DueDate = startDate.ToDateTime(TimeOnly.MinValue),
                        Status = TaskStatusEnum.Pending,
                        Session = (int)SessionTypeEnum.Evening,
                        CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                        PrescriptionId = newPrescription.Id,
                        IsTreatmentTask = true
                    });
                }

                var lastDate = startDate.AddDays((request.Prescriptions.DaysToTake.Value - 1));
                // Tạo task cho ngày mai nếu có thuốc kê cho buổi sáng, trưa, chiều, tối
                var tomorrow = startDate.AddDays(1);
                if (request.Prescriptions.DaysToTake == 1)
                {
                    lastDate = lastDate.AddDays(1); // Thêm ngày mai nếu kê đơn vào buổi trưa, chiều, tối
                                                    // Kiểm tra có thuốc kê cho buổi sáng, trưa, chiều, tối ngày mai
                    if (tomorrow <= lastDate)
                    {
                        // Kiểm tra và tạo task cho buổi sáng ngày mai nếu có thuốc kê cho sáng
                        if (hasMorningMedication && currentSession >= 1)
                        {
                            var morningMedications = sessionTasks[(int)SessionTypeEnum.Morning];
                            var medicationDetails = string.Join(", ", morningMedications.Select(m => $"{m.MedicationName} (Số liều: {m.Quantity})"));
                            var assignedUserId = await _userService.GetAssignedUserForCageAsync(cage.Id, tomorrow);
                            if (assignedUserId != null)
                            {
                                taskList.Add(new DataAccessObject.Models.Task
                                {
                                    TaskTypeId = taskType.Id,
                                    CageId = cage.Id,
                                    AssignedToUserId = assignedUserId.Value,
                                    CreatedByUserId = null,
                                    TaskName = $"Uống thuốc (Sáng)",
                                    Description = $"Điều trị cho {newPrescription.QuantityAnimal} con. Thuốc: {medicationDetails}.",
                                    PriorityNum = taskType.PriorityNum.Value,
                                    DueDate = tomorrow.ToDateTime(TimeOnly.MinValue),
                                    Status = TaskStatusEnum.Pending,
                                    Session = (int)SessionTypeEnum.Morning,
                                    CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                                    PrescriptionId = newPrescription.Id,
                                    IsTreatmentTask = true
                                });
                            }
                        }

                        // Tạo task cho buổi trưa ngày mai nếu có thuốc kê cho trưa
                        if (hasNoonMedication && currentSession >= 2)
                        {
                            var noonMedications = sessionTasks[(int)SessionTypeEnum.Noon];
                            var medicationDetails = string.Join(", ", noonMedications.Select(m => $"{m.MedicationName} (Số liều: {m.Quantity})"));
                            var assignedUserId = await _userService.GetAssignedUserForCageAsync(cage.Id, tomorrow);
                            if (assignedUserId != null)
                            {
                                taskList.Add(new DataAccessObject.Models.Task
                                {
                                    TaskTypeId = taskType.Id,
                                    CageId = cage.Id,
                                    AssignedToUserId = assignedUserId.Value,
                                    CreatedByUserId = null,
                                    TaskName = $"Uống thuốc (Trưa)",
                                    Description = $"Điều trị cho {newPrescription.QuantityAnimal} con. Thuốc: {medicationDetails}.",
                                    PriorityNum = taskType.PriorityNum.Value,
                                    DueDate = tomorrow.ToDateTime(TimeOnly.MinValue),
                                    Status = TaskStatusEnum.Pending,
                                    Session = (int)SessionTypeEnum.Noon,
                                    CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                                    PrescriptionId = newPrescription.Id,
                                    IsTreatmentTask = true
                                });
                            }
                        }

                        // Tạo task cho buổi chiều ngày mai nếu có thuốc kê cho chiều
                        if (hasAfternoonMedication && currentSession >= 3)
                        {
                            var afternoonMedications = sessionTasks[(int)SessionTypeEnum.Afternoon];
                            var medicationDetails = string.Join(", ", afternoonMedications.Select(m => $"{m.MedicationName} (Số liều: {m.Quantity})"));
                            var assignedUserId = await _userService.GetAssignedUserForCageAsync(cage.Id, tomorrow);
                            if (assignedUserId != null)
                            {
                                taskList.Add(new DataAccessObject.Models.Task
                                {
                                    TaskTypeId = taskType.Id,
                                    CageId = cage.Id,
                                    AssignedToUserId = assignedUserId.Value,
                                    CreatedByUserId = null,
                                    TaskName = $"Uống thuốc (Chiều)",
                                    Description = $"Điều trị cho  {newPrescription.QuantityAnimal}  con. Thuốc:  {medicationDetails} .",
                                    PriorityNum = taskType.PriorityNum.Value,
                                    DueDate = tomorrow.ToDateTime(TimeOnly.MinValue),
                                    Status = TaskStatusEnum.Pending,
                                    Session = (int)SessionTypeEnum.Afternoon,
                                    CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                                    PrescriptionId = newPrescription.Id,
                                    IsTreatmentTask = true
                                });
                            }
                        }

                        // Tạo task cho buổi tối ngày mai nếu có thuốc kê cho tối
                        if (hasEveningMedication && currentSession >= 4)
                        {
                            var eveningMedications = sessionTasks[(int)SessionTypeEnum.Evening];
                            var medicationDetails = string.Join(", ", eveningMedications.Select(m => $"{m.MedicationName} (Số liều: {m.Quantity})"));
                            var assignedUserId = await _userService.GetAssignedUserForCageAsync(cage.Id, tomorrow);
                            if (assignedUserId != null)
                            {
                                taskList.Add(new DataAccessObject.Models.Task
                                {
                                    TaskTypeId = taskType.Id,
                                    CageId = cage.Id,
                                    AssignedToUserId = assignedUserId.Value,
                                    CreatedByUserId = null,
                                    TaskName = $"Uống thuốc (Tối)",
                                    Description = $"Điều trị cho {newPrescription.QuantityAnimal} con. Thuốc: {medicationDetails}.",
                                    PriorityNum = taskType.PriorityNum.Value,
                                    DueDate = tomorrow.ToDateTime(TimeOnly.MinValue),
                                    Status = TaskStatusEnum.Pending,
                                    Session = (int)SessionTypeEnum.Evening,
                                    CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                                    PrescriptionId = newPrescription.Id,
                                    IsTreatmentTask = true
                                });
                            }
                        }
                    }
                }
                else
                {
                    // Kiểm tra có thuốc kê cho buổi sáng, trưa, chiều, tối ngày mai
                    if (tomorrow < lastDate)
                    {
                        // Kiểm tra và tạo task cho buổi sáng ngày mai nếu có thuốc kê cho sáng
                        if (hasMorningMedication)
                        {
                            var morningMedications = sessionTasks[(int)SessionTypeEnum.Morning];
                            var medicationDetails = string.Join(", ", morningMedications.Select(m => $"{m.MedicationName} (Số liều: {m.Quantity})"));
                            var assignedUserId = await _userService.GetAssignedUserForCageAsync(cage.Id, tomorrow);
                            if (assignedUserId != null)
                            {
                                taskList.Add(new DataAccessObject.Models.Task
                                {
                                    TaskTypeId = taskType.Id,
                                    CageId = cage.Id,
                                    AssignedToUserId = assignedUserId.Value,
                                    CreatedByUserId = null,
                                    TaskName = $"Uống thuốc (Sáng)",
                                    Description = $"Điều trị cho {newPrescription.QuantityAnimal} con. Thuốc: {medicationDetails}.",
                                    PriorityNum = taskType.PriorityNum.Value,
                                    DueDate = tomorrow.ToDateTime(TimeOnly.MinValue),
                                    Status = TaskStatusEnum.Pending,
                                    Session = (int)SessionTypeEnum.Morning,
                                    CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                                    PrescriptionId = newPrescription.Id,
                                    IsTreatmentTask = true
                                });
                            }
                        }

                        // Tạo task cho buổi trưa ngày mai nếu có thuốc kê cho trưa
                        if (hasNoonMedication)
                        {
                            var noonMedications = sessionTasks[(int)SessionTypeEnum.Noon];
                            var medicationDetails = string.Join(", ", noonMedications.Select(m => $"{m.MedicationName} (Số liều: {m.Quantity})"));
                            var assignedUserId = await _userService.GetAssignedUserForCageAsync(cage.Id, tomorrow);
                            if (assignedUserId != null)
                            {
                                taskList.Add(new DataAccessObject.Models.Task
                                {
                                    TaskTypeId = taskType.Id,
                                    CageId = cage.Id,
                                    AssignedToUserId = assignedUserId.Value,
                                    CreatedByUserId = null,
                                    TaskName = $"Uống thuốc (Trưa)",
                                    Description = $"Điều trị cho {newPrescription.QuantityAnimal} con. Thuốc: {medicationDetails}.",
                                    PriorityNum = taskType.PriorityNum.Value,
                                    DueDate = tomorrow.ToDateTime(TimeOnly.MinValue),
                                    Status = TaskStatusEnum.Pending,
                                    Session = (int)SessionTypeEnum.Noon,
                                    CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                                    PrescriptionId = newPrescription.Id,
                                    IsTreatmentTask = true
                                });
                            }
                        }

                        // Tạo task cho buổi chiều ngày mai nếu có thuốc kê cho chiều
                        if (hasAfternoonMedication)
                        {
                            var afternoonMedications = sessionTasks[(int)SessionTypeEnum.Afternoon];
                            var medicationDetails = string.Join(", ", afternoonMedications.Select(m => $"{m.MedicationName} (Số liều: {m.Quantity})"));
                            var assignedUserId = await _userService.GetAssignedUserForCageAsync(cage.Id, tomorrow);
                            if (assignedUserId != null)
                            {
                                taskList.Add(new DataAccessObject.Models.Task
                                {
                                    TaskTypeId = taskType.Id,
                                    CageId = cage.Id,
                                    AssignedToUserId = assignedUserId.Value,
                                    CreatedByUserId = null,
                                    TaskName = $"Uống thuốc (Chiều)",
                                    Description = $"Điều trị cho  {newPrescription.QuantityAnimal}  con. Thuốc:  {medicationDetails} .",
                                    PriorityNum = taskType.PriorityNum.Value,
                                    DueDate = tomorrow.ToDateTime(TimeOnly.MinValue),
                                    Status = TaskStatusEnum.Pending,
                                    Session = (int)SessionTypeEnum.Afternoon,
                                    CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                                    PrescriptionId = newPrescription.Id,
                                    IsTreatmentTask = true
                                });
                            }
                        }

                        // Tạo task cho buổi tối ngày mai nếu có thuốc kê cho tối
                        if (hasEveningMedication)
                        {
                            var eveningMedications = sessionTasks[(int)SessionTypeEnum.Evening];
                            var medicationDetails = string.Join(", ", eveningMedications.Select(m => $"{m.MedicationName} (Số liều: {m.Quantity})"));
                            var assignedUserId = await _userService.GetAssignedUserForCageAsync(cage.Id, tomorrow);
                            if (assignedUserId != null)
                            {
                                taskList.Add(new DataAccessObject.Models.Task
                                {
                                    TaskTypeId = taskType.Id,
                                    CageId = cage.Id,
                                    AssignedToUserId = assignedUserId.Value,
                                    CreatedByUserId = null,
                                    TaskName = $"Uống thuốc (Tối)",
                                    Description = $"Điều trị cho {newPrescription.QuantityAnimal} con. Thuốc: {medicationDetails}.",
                                    PriorityNum = taskType.PriorityNum.Value,
                                    DueDate = tomorrow.ToDateTime(TimeOnly.MinValue),
                                    Status = TaskStatusEnum.Pending,
                                    Session = (int)SessionTypeEnum.Evening,
                                    CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                                    PrescriptionId = newPrescription.Id,
                                    IsTreatmentTask = true
                                });
                            }
                        }
                    }
                }
                // Lưu TaskDaily và Task
                if (taskList.Any())
                {
                    await _unitOfWork.Tasks.CreateListAsync(taskList);
                }
                await _unitOfWork.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw new Exception($"Failed to create renew prescription: {ex.Message}");
            }
        }


        public async Task<PagedResult<PrescriptionList>> GetPrescriptionsAsync(
    DateTime? startDate, DateTime? endDate, string? status, string? cageName, int pageNumber, int pageSize)
        {
            var query = _unitOfWork.Prescription
                .FindAll()
                .Include(p => p.MedicalSymtom).ThenInclude(ms => ms.FarmingBatch).ThenInclude(fb => fb.Cage)
                .Include(p => p.MedicalSymtom).ThenInclude(ms => ms.Disease)
                .Include(p => p.PrescriptionMedications)
                    .ThenInclude(pm => pm.Medication)
                .AsQueryable();

            // 🔹 Lọc theo ngày kê đơn (PrescribedDate)
            if (startDate.HasValue)
            {
                query = query.Where(p => p.PrescribedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.PrescribedDate <= endDate.Value);
            }

            // 🔹 Lọc theo trạng thái đơn thuốc
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            // 🔹 Lọc theo tên chuồng
            if (!string.IsNullOrEmpty(cageName))
            {
                query = query.Where(p => p.MedicalSymtom.FarmingBatch.Cage.Name.Contains(cageName));
            }

            // 🔹 Đếm tổng số đơn thuốc
            int totalCount = await query.CountAsync();

            // 🔹 Tính toán số trang
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 🔹 Phân trang và lấy dữ liệu
            var prescriptions = await query
                .OrderByDescending(p => p.PrescribedDate) // Sắp xếp theo ngày kê đơn mới nhất
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PrescriptionList
                {
                    Id = p.Id,
                    RecordId = p.MedicalSymtomId,
                    CageId = p.CageId,
                    QuantityInCage = p.MedicalSymtom.QuantityInCage,
                    PrescribedDate = p.PrescribedDate,
                    EndDate = p.EndDate,
                    Notes = p.Notes,
                    QuantityAnimal = p.QuantityAnimal,
                    RemainingQuantity = p.RemainingQuantity,
                    Status = p.Status,
                    DaysToTake = p.DaysToTake,
                    Price = p.Price,
                    Symptoms = string.Join(", ", p.MedicalSymtom.MedicalSymptomDetails.Select(d => d.Symptom.SymptomName)),
                    CageAnimalName = p.MedicalSymtom.FarmingBatch.Cage.Name,
                    Disease = p.MedicalSymtom.Diagnosis,
                    NameAnimal = p.MedicalSymtom.FarmingBatch.Name,
                    Medications = p.PrescriptionMedications.Select(pm => new PrescriptionMedicationModel
                    {
                        MedicationId = pm.MedicationId,
                        Medication = new MedicationModel
                        {
                            Name = pm.Medication.Name,
                            UsageInstructions = pm.Medication.UsageInstructions,
                            Price = pm.Medication.Price,
                            DoseQuantity = pm.Medication.DoseQuantity
                        },
                        Morning = pm.Morning,
                        Noon = pm.Noon,
                        Afternoon = pm.Afternoon,
                        Evening = pm.Evening,
                        Notes = pm.Notes
                    }).ToList()
                })
        .ToListAsync();

            // 🔹 Trả về dữ liệu dạng PagedResult<T>
            return new PagedResult<PrescriptionList>
            {
                Items = prescriptions,
                TotalItems = totalCount,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                HasNextPage = pageNumber < totalPages,
                HasPreviousPage = pageNumber > 1
            };
        }

        public async Task<List<PrescriptionModel>> GetPrescriptionsHistoryAsync(Guid medicalSymptomId)
        {
            // 🔹 Lấy triệu chứng để truy xuất farmingBatchId
            var medicalSymptom = await _unitOfWork.MedicalSymptom
                .FindByCondition(ms => ms.Id == medicalSymptomId)
                .Include(ms => ms.FarmingBatch)
                .FirstOrDefaultAsync();

            if (medicalSymptom == null)
                throw new ArgumentException("Medical symptom not found.");

            var farmingBatchId = medicalSymptom.FarmingBatchId;

            // 🔹 Lấy tất cả các đơn thuốc từ vụ nuôi đó (ngoại trừ trạng thái Cancelled)
            var prescriptions = await _unitOfWork.Prescription
                .FindByCondition(p => p.MedicalSymtom.FarmingBatchId == farmingBatchId && p.Status != PrescriptionStatusEnum.Cancelled)
                .Include(p => p.PrescriptionMedications)
                    .ThenInclude(pm => pm.Medication)
                .Include(p => p.MedicalSymtom)
                    .ThenInclude(ms => ms.Disease) // Lấy bệnh liên quan
                .ToListAsync();

            // 🔹 Chuyển đổi dữ liệu sang PrescriptionModel
            return prescriptions.Select(p => new PrescriptionModel
            {
                Id = p.Id,
                PrescribedDate = p.PrescribedDate,
                Status = p.Status,
                QuantityAnimal = p.QuantityAnimal,
                Notes = p.Notes,
                Price = p.Price,
                DaysToTake = p.DaysToTake,
                EndDate = p.EndDate,
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
                }).ToList(),
                Disease = p.MedicalSymtom.Diagnosis
            }).ToList();
        }

    }
}
