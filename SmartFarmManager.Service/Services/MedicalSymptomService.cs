using MailKit.Search;
using Microsoft.EntityFrameworkCore;
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

        public MedicalSymptomService(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
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
                ms.FarmingBatch.Species.Contains(searchTerm) ||
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
                NameAnimal = ms.FarmingBatch.Species,
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
                        PrescribedDate = updatedModel.Prescriptions.PrescribedDate,
                        Notes = updatedModel.Prescriptions.Notes,
                        DaysToTake = updatedModel.Prescriptions.DaysToTake,
                        Status = updatedModel.Prescriptions.Status,
                        QuantityAnimal = updatedModel.Prescriptions.QuantityAnimal.Value,
                        EndDate = updatedModel.Prescriptions.PrescribedDate.Value.AddDays((double)updatedModel.Prescriptions.DaysToTake),
                        Price = totalPrice
                    };

                    await _unitOfWork.Prescription.CreateAsync(newPrescription);
                    var newPrescriptionMedication = updatedModel.Prescriptions.Medications.Select(m => new PrescriptionMedication
                    {
                        PrescriptionId = newPrescription.Id,
                        MedicationId = m.MedicationId,
                        Morning = m.Morning,
                        Afternoon = m.Afternoon,
                        Evening = m.Evening,
                        Noon = m.Noon
                    }).ToList();
                    await _unitOfWork.PrescriptionMedications.CreateListAsync(newPrescriptionMedication);

                    //update affectedQuantity in farmingBatch
                    var symtom = await _unitOfWork.MedicalSymptom.FindByCondition(ms => ms.Id == updatedModel.Id).Include(ms => ms.FarmingBatch).FirstOrDefaultAsync();
                    var farmingBatch = await _unitOfWork.FarmingBatches.FindByCondition(c => c.Id == symtom.FarmingBatch.Id).FirstOrDefaultAsync();
                    farmingBatch.AffectedQuantity += updatedModel.Prescriptions.QuantityAnimal.Value;
                    await _unitOfWork.FarmingBatches.UpdateAsync(farmingBatch);



                    //create task in today and tomorow
                    // Lấy thời gian hiện tại và buổi hiện tại
                    var currentTime = DateTimeUtils.VietnamNow().TimeOfDay;
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
                    DateOnly startDate = DateOnly.FromDateTime(DateTimeUtils.VietnamNow());
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
                    if (currentSession <= 1 && currentSession > 0 && hasMorningMedication) // Buổi sáng
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
                            Status = currentSession == 1 ? TaskStatusEnum.InProgress : TaskStatusEnum.Pending,
                            Session = (int)SessionTypeEnum.Morning,
                            CreatedAt = DateTimeUtils.VietnamNow(),
                            PrescriptionId= newPrescription.Id,
                            IsTreatmentTask = true
                        });
                    }

                    // Kiểm tra và tạo task cho buổi trưa
                    if (currentSession <= 2 && currentSession > 0 && hasNoonMedication) // Buổi trưa
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
                            Status = currentSession == 2 ? TaskStatusEnum.InProgress : TaskStatusEnum.Pending,
                            Session = (int)SessionTypeEnum.Noon,
                            CreatedAt = DateTimeUtils.VietnamNow(),
                            PrescriptionId = newPrescription.Id,
                            IsTreatmentTask = true
                        });
                    }

                    // Kiểm tra và tạo task cho buổi chiều
                    if (currentSession <= 3 && currentSession > 0 && hasAfternoonMedication) // Buổi chiều
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
                            Status = currentSession == 3 ? TaskStatusEnum.InProgress : TaskStatusEnum.Pending,
                            Session = (int)SessionTypeEnum.Afternoon,
                            CreatedAt = DateTimeUtils.VietnamNow(),
                            PrescriptionId = newPrescription.Id,
                            IsTreatmentTask = true
                        });
                    }

                    // Kiểm tra và tạo task cho buổi tối
                    if (currentSession <= 4 && currentSession > 0  && hasEveningMedication) // Buổi tối
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
                            Status = currentSession == 4 ? TaskStatusEnum.InProgress : TaskStatusEnum.Pending,
                            Session = (int)SessionTypeEnum.Evening,
                            CreatedAt = DateTimeUtils.VietnamNow(),
                            PrescriptionId = newPrescription.Id,
                            IsTreatmentTask = true
                        });
                    }

                    var lastDate = startDate.AddDays((updatedModel.Prescriptions.DaysToTake.Value - 1));


                    // Tạo task cho ngày mai nếu có thuốc kê cho buổi sáng, trưa, chiều, tối
                    var tomorrow = startDate.AddDays(1);

                    // Kiểm tra có thuốc kê cho buổi sáng, trưa, chiều, tối ngày mai
                    if (tomorrow <= lastDate)
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
                                    CreatedAt = DateTimeUtils.VietnamNow(),
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
                                    CreatedAt = DateTimeUtils.VietnamNow(),
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
                                    CreatedAt = DateTimeUtils.VietnamNow(),
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
                                    CreatedAt = DateTimeUtils.VietnamNow(),
                                    PrescriptionId = newPrescription.Id,
                                    IsTreatmentTask = true
                                });
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

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                Console.WriteLine($"Error in CreateFarmingBatchAsync: {ex.Message}");
                throw new Exception("Failed to create Farming Batch. Details: " + ex.Message);
            }
        }
        public async Task<Guid?> CreateMedicalSymptomAsync(MedicalSymptomModel medicalSymptomModel)
        {
            // Lấy ngày hiện tại theo múi giờ Việt Nam
            DateOnly currentDate = DateOnly.FromDateTime(DateTimeUtils.VietnamNow());

            // Tìm giai đoạn phát triển hiện tại
            var growthStage = await _unitOfWork.GrowthStages
                .FindByCondition(gs => gs.FarmingBatchId == medicalSymptomModel.FarmingBatchId &&
                                       gs.AgeStartDate.HasValue &&
                                       gs.AgeEndDate.HasValue &&
                                       currentDate >= DateOnly.FromDateTime(gs.AgeStartDate.Value) &&
                                       currentDate <= DateOnly.FromDateTime(gs.AgeEndDate.Value))
                .FirstOrDefaultAsync();
            var farmingBatches = await _unitOfWork.FarmingBatches.FindByCondition(fb => fb.Id == medicalSymptomModel.FarmingBatchId).FirstOrDefaultAsync();
            if (medicalSymptomModel.AffectedQuantity > growthStage.Quantity - farmingBatches.AffectedQuantity) 
            {
                return null;
            }
            // Bước 1: Tạo đối tượng MedicalSymptom mà chưa có MedicalSymptomDetails và Pictures
            var medicalSymptom = new DataAccessObject.Models.MedicalSymptom
            {
                FarmingBatchId = medicalSymptomModel.FarmingBatchId,
                PrescriptionId = medicalSymptomModel.PrescriptionId,
                Status = MedicalSymptomStatuseEnum.Pending,
                AffectedQuantity = medicalSymptomModel.AffectedQuantity,
                Notes = medicalSymptomModel.Notes,
                CreateAt = DateTimeUtils.VietnamNow()
            };
            // Bước 2: Lưu đối tượng MedicalSymptom vào cơ sở dữ liệu
            await _unitOfWork.MedicalSymptom.CreateAsync(medicalSymptom);
            await _unitOfWork.CommitAsync();

            // Bước 3: Tạo MedicalSymptomDetails và Pictures với MedicalSymptomId
            var medicalSymptomDetails = medicalSymptomModel.MedicalSymptomDetails.Select(d => new DataAccessObject.Models.MedicalSymtomDetail
            {
                SymptomId = d.SymptomId,
                MedicalSymptomId = medicalSymptom.Id, // Gán ID sau khi lưu
            }).ToList();

            var pictures = medicalSymptomModel.Pictures.Select(p => new DataAccessObject.Models.Picture
            {
                RecordId = medicalSymptom.Id, // Gán ID sau khi lưu
                Image = p.Image,
                DateCaptured = p.DateCaptured
            }).ToList();



            // Bước 6: Cập nhật lại MedicalSymptom
            await _unitOfWork.Pictures.CreateListAsync(pictures);
            await _unitOfWork.MedicalSymptomDetails.CreateListAsync(medicalSymptomDetails);
            await _unitOfWork.CommitAsync();

            return medicalSymptom.Id;
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
                NameAnimal = medicalSymptom.FarmingBatch.Species,
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
