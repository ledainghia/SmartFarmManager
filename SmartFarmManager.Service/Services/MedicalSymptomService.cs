using FirebaseAdmin.Messaging;
using MailKit.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Date;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.MedicalSymptom;
using SmartFarmManager.Service.BusinessModels.Medication;
using SmartFarmManager.Service.BusinessModels.Picture;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SmartFarmManager.Service.Services
{
    public class MedicalSymptomService : IMedicalSymptomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly NotificationService notificationService;
        private readonly INotificationService _notificationUserService;
        private readonly IQuartzService _quartzService;
        private readonly EmailService _emailService;

        public MedicalSymptomService(IUnitOfWork unitOfWork, IUserService userService, NotificationService notificationService, IQuartzService quartzService, INotificationService notificationUserService, EmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            this.notificationService = notificationService;
            _quartzService = quartzService;
            _notificationUserService = notificationUserService;
            _emailService = emailService;
        }

        public async Task<IEnumerable<MedicalSymptomModel>> GetMedicalSymptomsAsync(string? status, DateTime? startDate, DateTime? endDate, string? searchTerm)
        {
            var query = _unitOfWork.MedicalSymptom
        .FindAll()
        .Include(p => p.Pictures)
        .Include(p => p.FarmingBatch)
        .Include(p => p.Prescriptions).ThenInclude(p => p.PrescriptionMedications).ThenInclude(pm => pm.Medication)
        .Include(p => p.MedicalSymptomDetails).ThenInclude(p => p.Symptom)

        .AsQueryable();
            // Lọc theo trạng thái nếu có
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(ms => ms.Status == status);
            }

            // Lọc theo ngày tạo (CreateAt) nếu có
            if (startDate.HasValue)
            {
                query = query.Where(ms => ms.CreateAt >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(ms => ms.CreateAt <= endDate.Value);
            }

            // Lọc theo từ khóa tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(ms =>
                    ms.Diagnosis.Contains(searchTerm) ||
                    ms.Notes.Contains(searchTerm) ||
                    ms.MedicalSymptomDetails.Any(md => md.Symptom.SymptomName.Contains(searchTerm)));
            }

            var symptoms = await query.ToListAsync();
            return symptoms.Select(ms => new MedicalSymptomModel
            {
                Id = ms.Id,
                FarmingBatchId = ms.FarmingBatchId,
                Diagnosis = ms.Diagnosis,
                Status = ms.Status,
                AffectedQuantity = ms.AffectedQuantity,
                Notes = ms.Notes,
                Quantity = ms.FarmingBatch?.Quantity ?? 0,
                NameAnimal = ms.FarmingBatch.Name,
                CreateAt = ms.CreateAt,
                Pictures = ms.Pictures.Select(p => new PictureModel
                {
                    Id = p.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList(),
                Prescriptions = ms.Prescriptions.Select(p => new PrescriptionModel
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
                    }).ToList()
                }).FirstOrDefault(),
                Symtom = string.Join(", ", ms.MedicalSymptomDetails.Select(d => d.Symptom.SymptomName))
            });
        }


        public async Task<bool> UpdateMedicalSymptomAsync(UpdateMedicalSymptomModel updatedModel)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingSymptom = await _unitOfWork.MedicalSymptom
                .GetByIdAsync(updatedModel.Id);

                if (existingSymptom == null)
                {
                    return false;
                }

                // Danh sách trạng thái hợp lệ
                var validStatuses = new[]
                {
            MedicalSymptomStatuseEnum.Rejected,
            MedicalSymptomStatuseEnum.Prescribed
        };

                // Kiểm tra trạng thái có hợp lệ không
                if (!validStatuses.Contains(updatedModel.Status))
                {
                    throw new ArgumentException($"Trạng thái không hợp lệ: {updatedModel.Status}");
                }
                // Cập nhật thông tin
                existingSymptom.Diagnosis = updatedModel.Diagnosis;
                existingSymptom.Status = updatedModel.Status;
                existingSymptom.Notes = updatedModel.Notes;
                var cage = await _unitOfWork.Cages.FindByCondition(c => c.IsDeleted == false && c.IsSolationCage == true).FirstOrDefaultAsync();
                Guid? newPrescriptionId = null;
                // Tạo mới Prescription nếu có
                if (updatedModel.Prescriptions != null)
                {
                    var medications = updatedModel.Prescriptions.Medications;

                    var totalPrice = medications.Sum(m =>
                    {
                        var medication = _unitOfWork.Medication.GetByIdAsync(m.MedicationId).Result; // Lấy thông tin thuốc

                        // Tính tổng số liều (Morning + Noon + Afternoon + Evening)
                        var totalDoses = m.Morning + m.Noon + m.Afternoon + m.Evening;

                        // Tính giá dựa trên tổng số liều và giá mỗi liều
                        return medication.PricePerDose.HasValue ? medication.PricePerDose.Value * totalDoses : 0;
                    });
                    // Kiểm tra trạng thái đơn thuốc có hợp lệ không
                    if (updatedModel.Prescriptions.Status != PrescriptionStatusEnum.Active)
                    {
                        throw new ArgumentException($"Trạng thái đơn thuốc không hợp lệ: {updatedModel.Prescriptions.Status}");
                    }
                    var newPrescription = new Prescription
                    {
                        MedicalSymtomId = updatedModel.Id,
                        CageId = cage.Id,
                        //PrescribedDate = updatedModel.Prescriptions.PrescribedDate,
                        PrescribedDate = DateTimeUtils.GetServerTimeInVietnamTime(),
                        Notes = updatedModel.Prescriptions.Notes,
                        DaysToTake = updatedModel.Prescriptions.DaysToTake,
                        Status = updatedModel.Prescriptions.Status,
                        QuantityAnimal = updatedModel.Prescriptions.QuantityAnimal.Value,
                        //EndDate = updatedModel.Prescriptions.PrescribedDate.Value.AddDays((double)updatedModel.Prescriptions.DaysToTake),
                        EndDate = DateTimeUtils.GetServerTimeInVietnamTime().AddDays((double)updatedModel.Prescriptions.DaysToTake),
                        Price = totalPrice * updatedModel.Prescriptions.DaysToTake * updatedModel.Prescriptions.QuantityAnimal.Value
                    };

                    await _unitOfWork.Prescription.CreateAsync(newPrescription);
                    var newPrescriptionMedication = updatedModel.Prescriptions.Medications.Select(m => new PrescriptionMedication
                    {
                        PrescriptionId = newPrescription.Id,
                        Notes=m.Notes,
                        MedicationId = m.MedicationId,
                        Morning = m.Morning,
                        Afternoon = m.Afternoon,
                        Evening = m.Evening,
                        Noon = m.Noon
                    }).ToList();
                    await _unitOfWork.PrescriptionMedications.CreateListAsync(newPrescriptionMedication);
                    newPrescriptionId = newPrescription.Id;
                    //update affectedQuantity in farmingBatch
                    var symtom = await _unitOfWork.MedicalSymptom.FindByCondition(ms => ms.Id == updatedModel.Id).Include(ms => ms.FarmingBatch).FirstOrDefaultAsync();
                    var farmingBatch = await _unitOfWork.FarmingBatches.FindByCondition(c => c.Id == symtom.FarmingBatch.Id).FirstOrDefaultAsync();
                    farmingBatch.AffectedQuantity += updatedModel.Prescriptions.QuantityAnimal.Value;
                    await _unitOfWork.FarmingBatches.UpdateAsync(farmingBatch);



                    //create task in today and tomorow
                    // Lấy thời gian hiện tại và buổi hiện tại
                    var currentTime = DateTimeUtils.GetServerTimeInVietnamTime().TimeOfDay;
                    var currentSession = SessionTime.GetCurrentSession(currentTime);

                    // Kiểm tra đơn thuốc có thuốc kê cho các buổi sáng, trưa, chiều, tối hay không
                    var hasMorningMedication = updatedModel.Prescriptions.Medications.Any(m => m.Morning > 0);
                    var hasNoonMedication = updatedModel.Prescriptions.Medications.Any(m => m.Noon > 0);
                    var hasAfternoonMedication = updatedModel.Prescriptions.Medications.Any(m => m.Afternoon > 0);
                    var hasEveningMedication = updatedModel.Prescriptions.Medications.Any(m => m.Evening > 0);

                    // Tạo danh sách TaskDaily và Task
                    var taskList = new List<DataAccessObject.Models.Task>();
                    var taskType = await _unitOfWork.TaskTypes.FindByCondition(t => t.TaskTypeName == "Cho uống thuốc").FirstOrDefaultAsync();

                    // Tạo task cho ngày hiện tại
                    DateOnly startDate = DateOnly.FromDateTime(DateTimeUtils.GetServerTimeInVietnamTime());
                    TimeSpan startTime = TimeSpan.Zero;
                    var assignedUserTodayId = await _userService.GetAssignedUserForCageAsync(cage.Id, startDate);

                    var medicationIds = updatedModel.Prescriptions.Medications.Select(m => m.MedicationId).ToList();

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

                    var lastDate = startDate.AddDays((updatedModel.Prescriptions.DaysToTake.Value - 1));
                    // Tạo task cho ngày mai nếu có thuốc kê cho buổi sáng, trưa, chiều, tối
                    var tomorrow = startDate.AddDays(1);
                    if (updatedModel.Prescriptions.DaysToTake == 1)
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
                    
                }
                await _unitOfWork.MedicalSymptom.UpdateAsync(existingSymptom);
                await _unitOfWork.CommitAsync();

                var firstTask = await _unitOfWork.Tasks
    .FindByCondition(t => t.PrescriptionId == newPrescriptionId)
    .OrderBy(t => t.CreatedAt)  // Ưu tiên sắp xếp theo CreatedAt trước
    .ThenBy(t => t.Session)      // Sau đó sắp xếp theo Session
    .FirstOrDefaultAsync();
                if(firstTask != null)
                {
                    var staffFarm = await _unitOfWork.Users
                        .FindByCondition(u => u.CageStaffs.Any(cs => cs.CageId == cage.Id))
                        .FirstOrDefaultAsync();
                    var notiType = await _unitOfWork.NotificationsTypes.FindByCondition(nt => nt.NotiTypeName == "Task").FirstOrDefaultAsync();
                    var notificationStaff = new DataAccessObject.Models.Notification
                    {
                        UserId = staffFarm.Id,
                        NotiTypeId = notiType.Id,
                        Content = $"Một ngày mới bắt đầu! Bạn có công việc mới được giao. Hãy kiểm tra danh sách nhiệm vụ và hoàn thành đúng thời gian nhé!",
                        Title = "Bạn nhận được công việc mới!",
                        CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                        IsRead = false,
                        TaskId = firstTask.Id,
                        CageId = cage.Id
                    };
                    //await notificationService.SendNotification(staffFarm.DeviceId, "Bạn nhận được công việc mới!", notificationStaff);
                    await _unitOfWork.Notifications.CreateAsync(notificationStaff);
                }
                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                Console.WriteLine($"Error : {ex.Message}");
                throw new Exception("Failed. Details: " + ex.Message);
            }
        }

        //public async Task<Guid?> CreateMedicalSymptomAsync(MedicalSymptomModel medicalSymptomModel)
        //{
        //    try
        //    {
        //        // Lấy ngày hiện tại theo múi giờ Việt Nam
        //        DateOnly currentDate = DateOnly.FromDateTime(DateTimeUtils.GetServerTimeInVietnamTime());

        //    // Tìm giai đoạn phát triển hiện tại
        //    var growthStage = await _unitOfWork.GrowthStages
        //        .FindByCondition(gs => gs.FarmingBatchId == medicalSymptomModel.FarmingBatchId &&
        //                               gs.AgeStartDate.HasValue &&
        //                               gs.AgeEndDate.HasValue &&
        //                               currentDate >= DateOnly.FromDateTime(gs.AgeStartDate.Value) &&
        //                               currentDate <= DateOnly.FromDateTime(gs.AgeEndDate.Value))
        //        .FirstOrDefaultAsync();
        //    var farmingBatches = await _unitOfWork.FarmingBatches.FindByCondition(fb => fb.Id == medicalSymptomModel.FarmingBatchId).Include(fb => fb.Cage).FirstOrDefaultAsync();
        //    if (medicalSymptomModel.AffectedQuantity > growthStage.Quantity - farmingBatches.AffectedQuantity)
        //    {
        //        return null;
        //    }
        //    // Bước 1: Tạo đối tượng MedicalSymptom mà chưa có MedicalSymptomDetails và Pictures
        //    var medicalSymptom = new DataAccessObject.Models.MedicalSymptom
        //    {
        //        FarmingBatchId = medicalSymptomModel.FarmingBatchId,
        //        PrescriptionId = medicalSymptomModel.PrescriptionId,
        //        Status = MedicalSymptomStatuseEnum.Pending,
        //        AffectedQuantity = medicalSymptomModel.AffectedQuantity,
        //        Notes = medicalSymptomModel.Notes,
        //        CreateAt = DateTimeUtils.GetServerTimeInVietnamTime()
        //    };
        //    // Bước 2: Lưu đối tượng MedicalSymptom vào cơ sở dữ liệu
        //    await _unitOfWork.MedicalSymptom.CreateAsync(medicalSymptom);
        //    await _unitOfWork.CommitAsync();

        //    // Bước 3: Tạo MedicalSymptomDetails và Pictures với MedicalSymptomId
        //    var medicalSymptomDetails = medicalSymptomModel.MedicalSymptomDetails.Select(d => new DataAccessObject.Models.MedicalSymtomDetail
        //    {
        //        SymptomId = d.SymptomId,
        //        MedicalSymptomId = medicalSymptom.Id, // Gán ID sau khi lưu
        //    }).ToList();

        //    var pictures = medicalSymptomModel.Pictures.Select(p => new DataAccessObject.Models.Picture
        //    {
        //        RecordId = medicalSymptom.Id, // Gán ID sau khi lưu
        //        Image = p.Image,
        //        DateCaptured = p.DateCaptured
        //    }).ToList();

        //        //Notification realtime
        //        var vetFarm = await _unitOfWork.Users
        //.FindByCondition(u => u.Role.RoleName == "Vet")
        //.Include(u => u.Role) // Đảm bảo lấy Role ngay từ đầu để tránh Lazy Loading
        //.FirstOrDefaultAsync();

        //        var notiType = await _unitOfWork.NotificationsTypes.FindByCondition(nt => nt.NotiTypeName == "MedicalSymptom").FirstOrDefaultAsync();
        //        var adminFarm = await _unitOfWork.Users
        //.FindByCondition(u => u.Role.RoleName == "Admin")
        //.Include(u => u.Role) // Đảm bảo lấy Role ngay từ đầu để tránh Lazy Loading
        //.FirstOrDefaultAsync();

        //        var notificationVet = new DataAccessObject.Models.Notification
        //    {
        //        UserId = vetFarm.Id,
        //        NotiTypeId = notiType.Id,
        //        Content = $"Một báo cáo triệu chứng mới từ {farmingBatches.Cage.Name} đã được gửi vào lúc {DateTimeUtils.GetServerTimeInVietnamTime()}.\r\nVui lòng kiểm tra và xử lý kịp thời để đảm bảo sức khỏe cho vật nuôi.",
        //        Title = "Bạn có báo cáo bệnh mới",
        //        CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
        //        IsRead = false,
        //        MedicalSymptomId = medicalSymptom.Id,
        //        CageId = farmingBatches.CageId
        //    };
        //    var notificationAdmin = new DataAccessObject.Models.Notification
        //    {
        //        UserId = adminFarm.Id,
        //        NotiTypeId = notiType.Id,
        //        Content = $"Một báo cáo triệu chứng mới từ {farmingBatches.Cage.Name} đã được gửi vào lúc {DateTimeUtils.GetServerTimeInVietnamTime()}.\r\nVui lòng kiểm tra và xử lý kịp thời để đảm bảo sức khỏe cho vật nuôi.",
        //        Title = "Bạn có báo cáo bệnh mới",
        //        CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
        //        IsRead = false,
        //        MedicalSymptomId = medicalSymptom.Id,
        //        CageId = farmingBatches.CageId
        //    };
        //    await notificationService.SendNotification(vetFarm.DeviceId, "Có báo cáo triệu chứng mới", notificationVet);
        //    await _unitOfWork.Notifications.CreateAsync(notificationVet);
        //    if(vetFarm.DeviceId == adminFarm.DeviceId)
        //        {
        //            await System.Threading.Tasks.Task.Delay(500);
        //        }
        //    await notificationService.SendNotification(adminFarm.DeviceId, "Có báo cáo triệu chứng mới", notificationAdmin);
        //    await _unitOfWork.Notifications.CreateAsync(notificationAdmin);

        //    // Bước 6: Cập nhật lại MedicalSymptom
        //    await _unitOfWork.Pictures.CreateListAsync(pictures);
        //    await _unitOfWork.MedicalSymptomDetails.CreateListAsync(medicalSymptomDetails);
        //    await _unitOfWork.CommitAsync();


        //    await _quartzService.CreateReminderJobs(medicalSymptom.Id,DateTimeOffset.Now.LocalDateTime);

        //    return medicalSymptom.Id;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error in Create Symptom: {ex.Message}");
        //        throw new Exception("Failed to create Symptom. Details: " + ex.Message);
        //    }
        //}
        public async Task<MedicalSymptomModel?> CreateMedicalSymptomAsync(MedicalSymptomModel medicalSymptomModel)
        {
            try
            {
                Console.WriteLine("📌 Bắt đầu CreateMedicalSymptomAsync...");

                // Lấy ngày hiện tại theo múi giờ Việt Nam
                DateOnly currentDate = DateOnly.FromDateTime(DateTimeUtils.GetServerTimeInVietnamTime());
                Console.WriteLine($"✅ Lấy ngày hiện tại: {currentDate}");

                // Tìm giai đoạn phát triển hiện tại
                var growthStage = await _unitOfWork.GrowthStages
                    .FindByCondition(gs => gs.FarmingBatchId == medicalSymptomModel.FarmingBatchId &&
                                           gs.AgeStartDate.HasValue &&
                                           gs.AgeEndDate.HasValue &&
                                           currentDate >= DateOnly.FromDateTime(gs.AgeStartDate.Value) &&
                                           currentDate <= DateOnly.FromDateTime(gs.AgeEndDate.Value))
                    .FirstOrDefaultAsync();
                Console.WriteLine($"✅ Tìm giai đoạn phát triển: {growthStage?.Id}");

                var farmingBatches = await _unitOfWork.FarmingBatches
                    .FindByCondition(fb => fb.Id == medicalSymptomModel.FarmingBatchId)
                    .Include(fb => fb.Cage)
                    .FirstOrDefaultAsync();
                Console.WriteLine($"✅ Tìm farming batch: {farmingBatches?.Id}");

                if (medicalSymptomModel.AffectedQuantity > growthStage.Quantity - farmingBatches.AffectedQuantity)
                {
                    Console.WriteLine("⛔ Affected quantity vượt quá số lượng cho phép.");
                    return null;
                }

                // Bước 1: Tạo đối tượng MedicalSymptom
                var medicalSymptom = new DataAccessObject.Models.MedicalSymptom
                {
                    FarmingBatchId = medicalSymptomModel.FarmingBatchId,
                    PrescriptionId = Guid.Empty,
                    Status = MedicalSymptomStatuseEnum.Pending,
                    AffectedQuantity = medicalSymptomModel.AffectedQuantity,
                    Notes = medicalSymptomModel.Notes,
                    CreateAt = DateTimeUtils.GetServerTimeInVietnamTime()
                };
                Console.WriteLine("✅ Đã tạo đối tượng MedicalSymptom.");

                // Bước 2: Lưu vào cơ sở dữ liệu
                await _unitOfWork.MedicalSymptom.CreateAsync(medicalSymptom);
                await _unitOfWork.CommitAsync();
                Console.WriteLine($"✅ Đã lưu MedicalSymptom với ID: {medicalSymptom.Id}");

                // Bước 3: Tạo MedicalSymptomDetails và Pictures
                var medicalSymptomDetails = medicalSymptomModel.MedicalSymptomDetails.Select(d => new DataAccessObject.Models.MedicalSymtomDetail
                {
                    SymptomId = d.SymptomId,
                    MedicalSymptomId = medicalSymptom.Id,
                }).ToList();

                var pictures = medicalSymptomModel.Pictures.Select(p => new DataAccessObject.Models.Picture
                {
                    RecordId = medicalSymptom.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList();

                Console.WriteLine($"✅ Tạo {medicalSymptomDetails.Count} MedicalSymptomDetails & {pictures.Count} Pictures.");

                // Notification realtime
                var vetFarm = await _unitOfWork.Users
                    .FindByCondition(u => u.Role.RoleName == "Vet")
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync();
                Console.WriteLine($"✅ Tìm user có role Vet: {vetFarm?.Id}");

                var notiType = await _unitOfWork.NotificationsTypes
                    .FindByCondition(nt => nt.NotiTypeName == "MedicalSymptom")
                    .FirstOrDefaultAsync();
                Console.WriteLine($"✅ Tìm notification type: {notiType?.Id}");

                var adminFarm = await _unitOfWork.Users
                    .FindByCondition(u => u.Role.RoleName == "Admin")
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync();
                Console.WriteLine($"✅ Tìm user có role Admin: {adminFarm?.Id}");

                var notificationVet = new DataAccessObject.Models.Notification
                {
                    UserId = vetFarm.Id,
                    NotiTypeId = notiType.Id,
                    Content = $"Một báo cáo triệu chứng mới từ {farmingBatches.Cage.Name} đã được gửi vào lúc {DateTimeUtils.GetServerTimeInVietnamTime()}.\r\nVui lòng kiểm tra và xử lý kịp thời để đảm bảo sức khỏe cho vật nuôi.",
                    Title = "Bạn có báo cáo bệnh mới",
                    CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                    IsRead = false,
                    MedicalSymptomId = medicalSymptom.Id,
                    CageId = farmingBatches.CageId
                };
                Console.WriteLine($"✅ Tạo noti cho Vet");
                var notificationAdmin = new DataAccessObject.Models.Notification
                {
                    UserId = adminFarm.Id,
                    NotiTypeId = notiType.Id,
                    Content = $"Một báo cáo triệu chứng mới từ {farmingBatches.Cage.Name} đã được gửi vào lúc {DateTimeUtils.GetServerTimeInVietnamTime()}.\r\nVui lòng kiểm tra và xử lý kịp thời để đảm bảo sức khỏe cho vật nuôi.",
                    Title = "Bạn có báo cáo bệnh mới",
                    CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                    IsRead = false,
                    MedicalSymptomId = medicalSymptom.Id,
                    CageId = farmingBatches.CageId
                };
                Console.WriteLine($"✅ Tạo noti cho Admin");
                //await notificationService.SendNotification(vetFarm.DeviceId, "Có báo cáo triệu chứng mới", notificationVet);
                //Console.WriteLine("✅ Đã gửi thông báo cho Vet.");

                //await _unitOfWork.Notifications.CreateAsync(notificationVet);


                //await notificationService.SendNotification(adminFarm.DeviceId, "Có báo cáo triệu chứng mới", notificationAdmin);
                //Console.WriteLine("✅ Đã gửi thông báo cho Admin.");

                await _unitOfWork.Notifications.CreateAsync(notificationAdmin);

                // Bước 6: Lưu MedicalSymptomDetails & Pictures
                await _unitOfWork.Pictures.CreateListAsync(pictures);
                await _unitOfWork.MedicalSymptomDetails.CreateListAsync(medicalSymptomDetails);
                await _unitOfWork.CommitAsync();
                Console.WriteLine("✅ Đã lưu MedicalSymptomDetails & Pictures.");

                await _quartzService.CreateReminderJobs(medicalSymptom.Id, DateTimeOffset.Now.LocalDateTime);
                Console.WriteLine("✅ Đã tạo ReminderJobs.");

                Console.WriteLine($"🎉 Hoàn thành CreateMedicalSymptomAsync! ID: {medicalSymptom.Id}");

                // Map dữ liệu trả về
                var response = new MedicalSymptomModel
                {
                    Id = medicalSymptom.Id,
                    FarmingBatchId = medicalSymptom.FarmingBatchId,
                    Symtom = medicalSymptomModel.Symtom,
                    Diagnosis = medicalSymptom.Diagnosis,
                    Status = medicalSymptom.Status,
                    AffectedQuantity = medicalSymptom.AffectedQuantity,
                    Notes = medicalSymptom.Notes,
                    CreateAt = medicalSymptom.CreateAt,
                    Pictures = pictures.Select(p => new PictureModel
                    {
                        Id = p.RecordId,
                        Image = p.Image,
                        DateCaptured = p.DateCaptured
                    }).ToList(),
                    Prescriptions = null // Hiện tại Prescription chưa được tạo, có thể cập nhật sau
                };
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⛔ Lỗi trong CreateMedicalSymptomAsync: {ex.Message}");
                throw new Exception("Failed to create Symptom. Details: " + ex.Message);
            }
        }

        public async System.Threading.Tasks.Task ProcessMedicalSymptomReminderAsync(Guid medicalSymptomId)
        {


            var medicalSymptom = await _unitOfWork.MedicalSymptom
                .FindByCondition(ms => ms.Id == medicalSymptomId)
                .FirstOrDefaultAsync();

            if (medicalSymptom == null) return;

            // Kiểm tra trạng thái của MedicalSymptom
            if (medicalSymptom.Status == MedicalSymptomStatuseEnum.Pending)
            {
                // Lấy thông tin bác sĩ
                var vetFarm = await _unitOfWork.Users
                    .FindByCondition(u => u.Role.RoleName == "Vet")
                    .FirstOrDefaultAsync();

                var notiType = await _unitOfWork.NotificationsTypes.FindByCondition(nt => nt.NotiTypeName == "MedicalSymptom").FirstOrDefaultAsync();

                // Gửi thông báo lần 1 nếu chưa gửi
                if (medicalSymptom.FirstReminderSentAt == null)
                {
                    var notification = new DataAccessObject.Models.Notification
                    {
                        UserId = vetFarm.Id,
                        NotiTypeId = notiType.Id,
                        Content = "Có báo cáo triệu chứng mới chưa được chuẩn đoán.",
                        CreatedAt = DateTimeUtils.GetServerTimeInVietnamTime(),
                        MedicalSymptomId = medicalSymptom.Id,
                        IsRead = false
                    };

                    await _notificationUserService.CreateNotificationAsync(notification);
                    await notificationService.SendNotification(vetFarm.DeviceId, "Nhắc nhở bác sĩ", notification);
                    await _emailService.SendReminderEmailAsync(vetFarm.Email, vetFarm.FullName, "Nhắc nhở bác sĩ",
               "Bạn có một báo cáo triệu chứng chưa được chuẩn đoán. Vui lòng kiểm tra ngay.");
                    medicalSymptom.FirstReminderSentAt = DateTimeUtils.GetServerTimeInVietnamTime();
                }
                // Gửi thông báo lần 2 nếu chưa gửi
                else
                {
                    var notification = new DataAccessObject.Models.Notification
                    {
                        UserId = vetFarm.Id,
                        NotiTypeId = notiType.Id,
                        Content = "Bác sĩ vẫn chưa chuẩn đoán triệu chứng. Cần phải hành động ngay.",
                        CreatedAt = DateTime.UtcNow,
                        MedicalSymptomId = medicalSymptom.Id,
                        IsRead = false
                    };

                    await _notificationUserService.CreateNotificationAsync(notification);
                    await notificationService.SendNotification(vetFarm.DeviceId, "Nhắc nhở bác sĩ lần 2", notification);
                    medicalSymptom.SecondReminderSentAt = DateTimeUtils.GetServerTimeInVietnamTime();
                    await _emailService.SendReminderEmailAsync(vetFarm.Email, vetFarm.FullName, "Nhắc nhở bác sĩ lần 2",
               "Bác sĩ vẫn chưa phản hồi về triệu chứng. Cần hành động ngay.");

                    // Gửi thông báo cho admin
                    var admin = await _unitOfWork.Users
                        .FindByCondition(u => u.Role.RoleName == "Admin")
                        .FirstOrDefaultAsync();

                    var adminNotification = new DataAccessObject.Models.Notification
                    {
                        UserId = admin.Id,
                        NotiTypeId = notiType.Id,
                        Content = "Triệu chứng vẫn chưa được chuẩn đoán. Cần admin can thiệp.",
                        CreatedAt = DateTime.UtcNow,
                        MedicalSymptomId = medicalSymptom.Id,
                        IsRead = false
                    };

                    await notificationService.SendNotification(admin.DeviceId, "Triệu chứng vẫn chưa được chuẩn đoán. Cần admin can thiệp.", adminNotification);
                    await _emailService.SendReminderEmailAsync(admin.Email, admin.FullName, "Cảnh báo từ hệ thống",
               "Triệu chứng chưa được chuẩn đoán. Cần sự can thiệp của admin.");
                }

                // Cập nhật lại MedicalSymptom
                await _unitOfWork.MedicalSymptom.UpdateAsync(medicalSymptom);
                await _unitOfWork.CommitAsync();
            }
        }

        public async Task<MedicalSymptomModel?> GetMedicalSymptomByIdAsync(Guid id)
        {
            var medicalSymptom = await _unitOfWork.MedicalSymptom.FindAll()
                .Where(m => m.Id == id)
                .Include(p => p.Pictures)
                .Include(p => p.FarmingBatch)
                .Include(p => p.Prescriptions).ThenInclude(p => p.PrescriptionMedications).ThenInclude(p => p.Medication)
                .FirstOrDefaultAsync();

            if (medicalSymptom == null)
            {
                return null;
            }

            return new MedicalSymptomModel
            {
                Id = medicalSymptom.Id,
                FarmingBatchId = medicalSymptom.FarmingBatchId,
                Diagnosis = medicalSymptom.Diagnosis,
                Status = medicalSymptom.Status,
                AffectedQuantity = medicalSymptom.AffectedQuantity,
                Notes = medicalSymptom.Notes,
                Quantity = medicalSymptom.FarmingBatch.Quantity,
                NameAnimal = medicalSymptom.FarmingBatch.Name,
                CreateAt = medicalSymptom.CreateAt,
                Pictures = medicalSymptom.Pictures.Select(p => new PictureModel
                {
                    Id = p.Id,
                    Image = p.Image,
                    DateCaptured = p.DateCaptured
                }).ToList(),
                Prescriptions = medicalSymptom.Prescriptions.Select(p => new PrescriptionModel
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
                    }).ToList()
                }).FirstOrDefault(),
            };
        }
        public async Task<IEnumerable<MedicalSymptomModel>> GetMedicalSymptomsByStaffAndBatchAsync(Guid? staffId, Guid? farmBatchId)
        {
            // Lấy danh sách CageStaff theo Staff ID
            var cageStaffs = await _unitOfWork.CageStaffs
                .FindByCondition(cs => cs.StaffFarmId == staffId, trackChanges: false, cs => cs.Cage)
                .ToListAsync();

            if (!cageStaffs.Any())
            {
                return Enumerable.Empty<MedicalSymptomModel>();
            }

            // Lấy danh sách Cage IDs từ CageStaff
            var cageIds = cageStaffs.Select(cs => cs.CageId).Distinct();

            // Kiểm tra xem FarmBatchId có thuộc Cage của Staff không
            var farmingBatch = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => cageIds.Contains(fb.CageId) && fb.Id == farmBatchId)
                .Include(p => p.MedicalSymptoms)
                .ThenInclude(p => p.MedicalSymptomDetails).ThenInclude(p => p.Symptom)
                .FirstOrDefaultAsync();

            if (farmingBatch == null)
            {
                return Enumerable.Empty<MedicalSymptomModel>();
            }

            // Lấy danh sách Medical Symptoms từ Farming Batch
            return farmingBatch.MedicalSymptoms.Select(ms => new MedicalSymptomModel
            {
                Id = ms.Id,
                FarmingBatchId = ms.FarmingBatchId,
                Diagnosis = ms.Diagnosis,
                Status = ms.Status,
                AffectedQuantity = ms.AffectedQuantity,
                Quantity = farmingBatch.Quantity,
                Notes = ms.Notes,
                CreateAt = ms.CreateAt,
                Symtom = string.Join(", ", ms.MedicalSymptomDetails.Select(md => md.Symptom.SymptomName)) // Nối triệu chứng
            });
        }
    }
}
