using SmartFarmManager.DataAccessObject.Models;
using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Task;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SmartFarmManager.Repository.Repositories;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Shared;
using Task = SmartFarmManager.DataAccessObject.Models.Task;

namespace SmartFarmManager.Service.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly NotificationService _notificationService;

        public TaskService(IUnitOfWork unitOfWork, IMapper mapper, NotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<bool> CreateTaskRecurringAsync(CreateTaskRecurringModel model)
        {
            // 1. Kiểm tra Cage có thuộc FarmingBatch đang hoạt động không
            var farmingBatch = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.CageId == model.CageId && fb.Status == "Active")
                .Include(fb => fb.GrowthStages)
                .FirstOrDefaultAsync();

            if (farmingBatch == null)
            {
                throw new ArgumentException("No active farming batch found for the specified cage.");
            }

            // 2. Kiểm tra StartAt và EndAt có nằm trong khoảng của GrowthStage không
            var validGrowthStage = farmingBatch.GrowthStages
                .FirstOrDefault(gs => model.StartAt.Date >= gs.AgeStartDate.Value.Date && model.EndAt.Date <= gs.AgeEndDate.Value.Date);

            if (validGrowthStage == null)
            {
                throw new ArgumentException("The specified date range does not align with any active growth stage.");
            }

            // 3. Kiểm tra trùng lặp trong khoảng ngày
            foreach (var session in model.Sessions)
            {
                var duplicateTaskDailyExists = await _unitOfWork.TaskDailies
                    .FindByCondition(td => td.GrowthStageId == validGrowthStage.Id &&
                                           td.Session == session &&
                                           td.TaskTypeId == model.TaskTypeId &&
                                           td.StartAt <= model.EndAt && td.EndAt >= model.StartAt)
                    .AnyAsync();

                if (duplicateTaskDailyExists)
                {
                    throw new InvalidOperationException($"A TaskDaily with TaskTypeId '{model.TaskTypeId}' already exists in session '{session}' for the specified time range.");
                }
            }

            var taskDailies = new List<TaskDaily>();

            foreach (var session in model.Sessions)
            {
                var taskDaily = new TaskDaily
                {
                    GrowthStageId = validGrowthStage.Id,
                    TaskTypeId = model.TaskTypeId,
                    TaskName = model.TaskName,
                    Description = model.Description,
                    Session = session,
                    StartAt = model.StartAt,
                    EndAt = model.EndAt
                };

                taskDailies.Add(taskDaily);
            }

            await _unitOfWork.TaskDailies.CreateListAsync(taskDailies);
            await _unitOfWork.CommitAsync();

            return true;
        }


        public async Task<bool> CreateTaskAsync(CreateTaskModel model)
        {
            // 1. Validate DueDate
            if (model.DueDate < DateTimeUtils.VietnamNow())
            {
                throw new ArgumentException("DueDate must be in the future.");
            }

            // 2. Validate TaskTypeId
            var taskType = await _unitOfWork.TaskTypes.FindAsync(x => x.Id == model.TaskTypeId);
            if (taskType == null)
            {
                throw new ArgumentException("Invalid TaskTypeId.");
            }

            // 3. Lấy FarmingBatch đang hoạt động liên quan đến Cage
            var farmingBatch = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.CageId == model.CageId && fb.Status == FarmingBatchStatusEnum.Active)
                .Include(fb => fb.GrowthStages)
                .FirstOrDefaultAsync();

            if (farmingBatch == null)
            {
                throw new ArgumentException($"No active FarmingBatch found for Cage {model.CageId}.");
            }

            // 4. Lấy GrowthStage phù hợp với ngày
            var validGrowthStage = farmingBatch.GrowthStages
                .FirstOrDefault(gs => gs.AgeStartDate.Value.Date <= model.DueDate.Date && gs.AgeEndDate.Value.Date >= model.DueDate.Date);

            if (validGrowthStage == null)
            {
                throw new ArgumentException($"No valid GrowthStage found for the specified date {model.DueDate.ToShortDateString()}.");
            }

            // 5. Get Assigned User for the Cage
            Guid? assignedUserId = null;

            // Check if there is a leave request for the staff managing the cage
            var cageStaff = await _unitOfWork.CageStaffs
                .FindByCondition(cs => cs.CageId == model.CageId)
                .FirstOrDefaultAsync();

            if (cageStaff != null)
            {
                var leaveRequest = await _unitOfWork.LeaveRequests
                    .FindByCondition(lr => lr.StaffFarmId == cageStaff.StaffFarmId &&
                                           lr.StartDate.Date <= model.DueDate.Date &&
                                           lr.EndDate.Date >= model.DueDate.Date &&
                                           lr.Status == LeaveRequestStatusEnum.Approved)
                    .FirstOrDefaultAsync();

                if (leaveRequest != null)
                {
                    // If a leave request exists and approved, use the temporary user
                    assignedUserId = leaveRequest.UserTempId;
                }
                else
                {
                    // Otherwise, assign to the default staff
                    assignedUserId = cageStaff.StaffFarmId;
                }
            }

            // If no staff is assigned to the cage, throw an exception
            if (assignedUserId == null)
            {
                throw new InvalidOperationException($"No staff is assigned to the cage {model.CageId}.");
            }

            // 6. Ensure AssignedToUserId is valid and active
            var assignedUser = await _unitOfWork.Users.FindAsync(x => x.Id == assignedUserId);
            if (assignedUser == null || assignedUser.IsActive == null || (bool)assignedUser.IsActive == false)
            {
                throw new ArgumentException("Invalid or inactive AssignedToUserId.");
            }

            var tasksToCreate = new List<DataAccessObject.Models.Task>();

            foreach (var session in model.Session)
            {
                // 7. Validate Session
                if (!Enum.IsDefined(typeof(SessionTypeEnum), session))
                {
                    throw new ArgumentException($"Invalid session value '{session}'.");
                }

                // 8. Check for duplicate TaskTypeId in the same session within TaskDaily
                var duplicateTaskDailyExists = await _unitOfWork.TaskDailies
                    .FindByCondition(td => td.GrowthStageId == validGrowthStage.Id &&
                                           td.Session == session &&
                                           td.TaskTypeId == model.TaskTypeId)
                    .AnyAsync();

                if (duplicateTaskDailyExists)
                {
                    throw new InvalidOperationException($"A TaskDaily with TaskTypeId '{model.TaskTypeId}' already exists in session '{session}'.");
                }

                // 9. Check if a task with the same TaskType exists for the same cage, session, and day
                var taskWithSameTypeExists = await _unitOfWork.Tasks
                    .FindByCondition(t => t.DueDate.HasValue &&
                                          t.DueDate.Value.Date == model.DueDate.Date &&
                                          t.Session == session &&
                                          t.CageId == model.CageId &&
                                          t.TaskTypeId == model.TaskTypeId &&
                                          t.CompletedAt == null)
                    .AnyAsync();

                if (taskWithSameTypeExists)
                {
                    throw new InvalidOperationException($"A task of type '{taskType.TaskTypeName}' already exists in cage {model.CageId} on {model.DueDate.Date.ToShortDateString()} during session '{session}'.");
                }

                // 10. Create Task
                var task = new DataAccessObject.Models.Task
                {
                    Id = Guid.NewGuid(),
                    TaskTypeId = model.TaskTypeId,
                    CageId = model.CageId,
                    AssignedToUserId = assignedUserId.Value,
                    CreatedByUserId = model.CreatedByUserId,
                    TaskName = model.TaskName,
                    PriorityNum = (int)taskType.PriorityNum,
                    Description = model.Description,
                    DueDate = model.DueDate,
                    Session = session,
                    CreatedAt = DateTimeUtils.VietnamNow(),
                    Status = TaskStatusEnum.Pending
                };

                tasksToCreate.Add(task);
            }

            // Save all tasks
            await _unitOfWork.Tasks.CreateListAsync(tasksToCreate);
            await _unitOfWork.CommitAsync();

            // Send Notifications
            foreach (var task in tasksToCreate)
            {
                var message = $"Task '{task.TaskName}' has been created and assigned to {assignedUser.FullName}.";
                await _notificationService.SendNotificationToUser(task.AssignedToUserId.ToString(), message);
            }

            return true;
        }




        public async Task<bool> UpdateTaskPriorityAsync(Guid taskId, UpdateTaskPriorityModel model)
        {

            var task = await _unitOfWork.Tasks.FindAsync(x => x.Id == taskId);
            if (task == null)
            {
                throw new KeyNotFoundException("Task not found.");
            }

            if (task.Session != model.Session)
            {
                var taskDate = task.DueDate?.Date;
                if (taskDate == null)
                {
                    throw new ArgumentException("Task does not have a valid DueDate.");
                }

                // Lấy tất cả task trong cùng ngày chung session khác (cùng ngày, chung session)
                var tasksInOldSession = await _unitOfWork.Tasks
                        .FindByCondition(t => t.DueDate.HasValue &&
                                  t.DueDate.Value.Date == taskDate.Value.Date &&
                                  t.Session == task.Session &&
                                  t.CageId == task.CageId)
                        .OrderBy(t => t.PriorityNum)
                        .ToListAsync();
                // Lấy tất cả task trong cùng ngày nhưng có session khác (cùng ngày, khác session)
                var tasksInNewSession = await _unitOfWork.Tasks
                        .FindByCondition(t => t.DueDate.HasValue &&
                                  t.DueDate.Value.Date == taskDate.Value.Date &&
                                  t.Session == model.Session &&
                                  t.CageId == task.CageId)
                        .OrderBy(t => t.PriorityNum)
                        .ToListAsync();

                // 1. Cập nhật PriorityNum của các task trong session cũ
                foreach (var oldTask in tasksInOldSession)
                {
                    if (oldTask.PriorityNum > task.PriorityNum)
                    {
                        oldTask.PriorityNum -= 1;
                    }
                }

                // 2. Cập nhật PriorityNum của các task trong session mới
                foreach (var newTask in tasksInNewSession)
                {
                    if (newTask.PriorityNum >= model.NewPriority)
                    {
                        newTask.PriorityNum += 1;
                    }
                }
                task.Session = model.Session;
                task.PriorityNum = model.NewPriority;
                await _unitOfWork.Tasks.UpdateAsync(task);
                await _unitOfWork.CommitAsync();
                return true;



            }
            else
            {
                var taskDate = task.DueDate?.Date;
                if (taskDate == null)
                {
                    throw new ArgumentException("Task does not have a valid DueDate.");
                }

                // Lấy tất cả các task trong cùng ngày và cùng session
                var tasksInSameSession = await _unitOfWork.Tasks
                    .FindByCondition(t => t.DueDate.HasValue &&
                                          t.DueDate.Value.Date == taskDate.Value.Date &&
                                          t.Session == task.Session &&
                                          t.CageId == task.CageId)
                    .OrderBy(t => t.PriorityNum)
                    .ToListAsync();

                // Giảm PriorityNum của các task có PriorityNum > PriorityNum hiện tại
                foreach (var t in tasksInSameSession.Where(t => t.PriorityNum > task.PriorityNum))
                {
                    t.PriorityNum -= 1;
                }

                // Tăng PriorityNum của các task có PriorityNum >= newPriority
                foreach (var t in tasksInSameSession.Where(t => t.PriorityNum >= model.NewPriority))
                {
                    t.PriorityNum += 1;
                }
                task.PriorityNum = model.NewPriority;

                await _unitOfWork.Tasks.UpdateAsync(task);
                await _unitOfWork.CommitAsync();
                return true;

            }
        }



        //change status of task by task id and status id
        public async Task<bool> ChangeTaskStatusAsync(Guid taskId, string? status)
        {
            var task = await _unitOfWork.Tasks.FindAsync(x => x.Id == taskId);
            if (status == null ||
    (status != TaskStatusEnum.Pending &&
     status != TaskStatusEnum.InProgress &&
     status != TaskStatusEnum.Done &&
     status != TaskStatusEnum.Overdue))
            {
                throw new ArgumentException("Không đúng status");
            }


            if (status == TaskStatusEnum.Done)
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            task.Status = status;
            await _unitOfWork.Tasks.UpdateAsync(task);
            await _unitOfWork.CommitAsync();

            return true;
        }

        //get task filter
        public async Task<List<TaskModel>> GetTasksAsync(TaskModel filter)
        {
            var query = _unitOfWork.Tasks.FindAll();

            if (filter.TaskTypeId != Guid.Empty)
                query = query.Where(t => t.TaskTypeId == filter.TaskTypeId);

            if (filter.CageId != Guid.Empty)
                query = query.Where(t => t.CageId == filter.CageId);

            if (filter.AssignedToUserId != Guid.Empty)
                query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId);

            if (filter.CreatedByUserId != Guid.Empty)
                query = query.Where(t => t.CreatedByUserId == filter.CreatedByUserId);

            if (!string.IsNullOrEmpty(filter.TaskName))
                query = query.Where(t => t.TaskName.Contains(filter.TaskName));

            if (filter.PriorityNum.HasValue && filter.PriorityNum.Value != 0)
                query = query.Where(t => t.PriorityNum == filter.PriorityNum);

            if (!string.IsNullOrEmpty(filter.Description))
                query = query.Where(t => t.Description.Contains(filter.Description));

            if (filter.DueDate.HasValue)
                query = query.Where(t => t.DueDate == filter.DueDate);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(t => t.Status == filter.Status);

            if (filter.Session.HasValue && filter.Session.Value != 0)
                query = query.Where(t => t.Session == filter.Session);

            if (filter.CompletedAt.HasValue)
                query = query.Where(t => t.CompletedAt == filter.CompletedAt);

            if (filter.CreatedAt.HasValue)
                query = query.Where(t => t.CreatedAt == filter.CreatedAt);

            var listTask = await query.OrderBy(t => t.PriorityNum).ToListAsync();
            var taskModel = _mapper.Map<List<TaskModel>>(listTask);
            return taskModel;
        }

        public async Task<List<TaskResponse>> GetTasksForUserWithStateAsync(Guid userId, Guid cageId, DateTime? specificDate = null)
        {
            // Ngày hiện tại và ngày mặc định
            //chỉ ngày nay với mai?????????????????
            var today = DateTime.Today;
            var filterDate = specificDate ?? today;

            // Lấy tất cả task trong ngày chỉ định
            var allTasks = await _unitOfWork.Tasks.FindAll()
                .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == filterDate && t.CageId == cageId)
                .OrderBy(t => t.PriorityNum)
                .ToListAsync();

            // Tìm task có độ ưu tiên cao nhất chưa hoàn thành của tất cả user
            var highestUnfinishedTask = allTasks
                .Where(t => t.CompletedAt == null)
                .OrderBy(t => t.PriorityNum)
                .FirstOrDefault();

            // Lấy danh sách task của user hiện tại
            var userTasks = allTasks
                .Where(t => t.AssignedToUserId == userId)
                .OrderBy(t => t.PriorityNum)
                .ToList();

            // Danh sách trả về
            var taskResponses = new List<TaskResponse>();

            // Trường hợp user A
            if (userId == highestUnfinishedTask?.AssignedToUserId)
            {
                foreach (var task in userTasks)
                {
                    bool isDisabled = task.PriorityNum > highestUnfinishedTask.PriorityNum;
                    string reason = isDisabled
                        ? $"Blocked by Task {highestUnfinishedTask.PriorityNum} của bạn"
                        : string.Empty;

                    taskResponses.Add(new TaskResponse
                    {
                        TaskId = task.Id,
                        TaskName = task.TaskName,
                        PriorityNum = task.PriorityNum,
                        IsDisabled = isDisabled,
                        Status = task.Status,
                        DueDate = task.DueDate,
                        Reason = reason
                    });
                }
            }
            else
            {
                // Trường hợp user B
                foreach (var task in userTasks)
                {
                    bool isDisabled = highestUnfinishedTask != null;
                    string reason = isDisabled
                        ? $"Blocked by Task {highestUnfinishedTask.PriorityNum} của Người A"
                        : string.Empty;

                    taskResponses.Add(new TaskResponse
                    {
                        TaskId = task.Id,
                        TaskName = task.TaskName,
                        PriorityNum = task.PriorityNum,
                        IsDisabled = isDisabled,
                        Status = task.Status,
                        DueDate = task.DueDate,
                        Reason = reason
                    });
                }
            }

            // Nếu có task chưa hoàn thành của Người A và user hiện tại không phải Người A
            if (highestUnfinishedTask != null && userId != highestUnfinishedTask.AssignedToUserId)
            {
                taskResponses.Insert(0, new TaskResponse
                {
                    TaskId = highestUnfinishedTask.Id,
                    TaskName = highestUnfinishedTask.TaskName,
                    PriorityNum = highestUnfinishedTask.PriorityNum,
                    IsDisabled = false, // Task global hiển thị bình thường
                    Status = highestUnfinishedTask.Status,
                    DueDate = highestUnfinishedTask.DueDate,
                    Reason = "Global task with highest priority"
                });
            }

            return taskResponses;
        }

        public async Task<List<NextTaskModel>> GetNextTasksForCagesWithStatsAsync(Guid userId)
        {
            // Ngày hôm nay
            var today = DateTime.Now.Date;

            // Lấy tất cả task trong ngày của user
            var userTasksToday = await _unitOfWork.Tasks.FindAll()
                .Where(t => t.AssignedToUserId == userId && t.DueDate.HasValue && t.DueDate.Value.Date == today.Date)
                .Include(u => u.AssignedToUser)
                .Include(c => c.Cage)
                .ToListAsync();

            // Nhóm các task theo CageId
            var groupedTasks = userTasksToday
                .GroupBy(t => t.CageId)
                .Select(g => new
                {
                    CageId = g.Key,
                    TotalTasks = g.Count(), // Tổng số task của từng chuồng
                    CompletedTasks = g.Count(t => t.CompletedAt != null), // Số task đã hoàn thành của từng chuồng
                    NextTask = g.Where(t => t.CompletedAt == null) // Chỉ lấy task chưa hoàn thành
                                .OrderBy(t => t.PriorityNum) // Sắp xếp theo mức ưu tiên
                                .ThenBy(t => t.DueDate) // Sắp xếp thêm theo ngày
                                .FirstOrDefault() // Lấy task tiếp theo
                })
                .ToList();

            // Tạo danh sách NextTaskModel
            var result = groupedTasks
                .Where(g => g.NextTask != null) // Chỉ lấy nhóm có task tiếp theo
                .Select(g => new NextTaskModel
                {
                    TaskId = g.NextTask.Id,
                    CageId = g.CageId,
                    TaskName = g.NextTask.TaskName,
                    Cagename = g.NextTask.Cage?.Name ?? "Unknown Cage", // Tên chuồng
                    AssignName = g.NextTask.AssignedToUser?.FullName ?? "Unknown User", // Tên người được gán
                    PriorityNum = g.NextTask.PriorityNum,
                    Status = g.NextTask.Status,
                    DueDate = g.NextTask.DueDate,
                    Total = g.TotalTasks, // Tổng số task của chuồng
                    TaskDone = g.CompletedTasks // Số task đã hoàn thành của chuồng
                })
                .ToList();

            return result;
        }




        public async Task<PagedResult<TaskDetailModel>> GetFilteredTasksAsync(TaskFilterModel filter)
        {
            // Query từ repository
            var query = _unitOfWork.Tasks.FindAll(false, x => x.AssignedToUser, x => x.TaskType, x => x.StatusLogs).Include(x => x.Cage).Include(x => x.StatusLogs).AsQueryable();

            // Áp dụng bộ lọc
            if (!string.IsNullOrEmpty(filter.TaskName))
            {
                query = query.Where(t => t.TaskName.Contains(filter.TaskName));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(t => t.Status == filter.Status);
            }

            if (filter.TaskTypeId.HasValue)
            {
                query = query.Where(t => t.TaskTypeId == filter.TaskTypeId.Value);
            }

            if (filter.CageId.HasValue)
            {
                query = query.Where(t => t.CageId == filter.CageId.Value);
            }

            if (filter.AssignedToUserId.HasValue)
            {
                query = query.Where(t => t.AssignedToUserId == filter.AssignedToUserId.Value);
            }

            if (filter.DueDateFrom.HasValue)
            {
                query = query.Where(t => t.DueDate >= filter.DueDateFrom.Value);
            }

            if (filter.DueDateTo.HasValue)
            {
                query = query.Where(t => t.DueDate <= filter.DueDateTo.Value);
            }

            if (filter.PriorityNum.HasValue)
            {
                query = query.Where(t => t.PriorityNum == filter.PriorityNum.Value);
            }

            if (filter.Session.HasValue)
            {
                query = query.Where(t => t.Session == filter.Session.Value);
            }

            if (filter.CompletedAt.HasValue)
            {
                query = query.Where(t => t.CompletedAt.HasValue && t.CompletedAt.Value.Date == filter.CompletedAt.Value.Date);
            }

            if (filter.CreatedAt.HasValue)
            {
                query = query.Where(t => t.CreatedAt.Value.Date == filter.CreatedAt.Value.Date);
            }

            // Sắp xếp dữ liệu
            query = query.OrderBy(t => t.CageId)
                         .ThenBy(t => t.DueDate.Value.Date)
                         .ThenBy(t => t.Session)
                         .ThenBy(t => t.PriorityNum);

            // Tổng số phần tử
            var totalItems = await query.CountAsync();

            // Phân trang và chọn các trường cần thiết
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(t => new TaskDetailModel
                {
                    Id = t.Id,
                    CageId = t.CageId,
                    CageName = t.Cage.Name,
                    TaskName = t.TaskName,
                    Description = t.Description,
                    PriorityNum = t.PriorityNum,
                    DueDate = t.DueDate,
                    Status = t.Status,
                    Session = t.Session,
                    CompletedAt = t.CompletedAt,
                    CreatedAt = t.CreatedAt,
                    AssignedToUser = t.AssignedToUser == null ? null : new UserResponseModel
                    {
                        UserId = t.AssignedToUser.Id,
                        FullName = t.AssignedToUser.FullName,
                        Email = t.AssignedToUser.Email,
                        PhoneNumber = t.AssignedToUser.PhoneNumber
                    },
                    TaskType = t.TaskType == null ? null : new TaskTypeResponseModel
                    {
                        TaskTypeId = t.TaskType.Id,
                        TaskTypeName = t.TaskType.TaskTypeName
                    },
                    StatusLogs = t.StatusLogs.Select(s => new StatusLogResponseModel
                    {
                        Status = s.Status,
                        UpdatedAt = s.UpdatedAt
                    }).ToList()
                })
                .ToListAsync();

            var result = new PaginatedList<TaskDetailModel>(items, totalItems, filter.PageNumber, filter.PageSize);
            // Trả về kết quả
            return new PagedResult<TaskDetailModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage
            };
        }


        public async Task<TaskDetailModel> GetTaskDetailAsync(Guid taskId)
        {
            var task = await _unitOfWork.Tasks
                            .FindByCondition(t => t.Id == taskId)
                            .Include(t => t.Cage)
                            .Include(t => t.AssignedToUser)
                            .Include(t => t.TaskType)
                            .Include(t => t.StatusLogs)
                            .FirstOrDefaultAsync();
            if (task == null)
            {
                return null;
            }

            // Map Task sang TaskDetailResponse
            return new TaskDetailModel
            {
                Id = task.Id,
                CageId = task.CageId,
                CageName = task.Cage.Name,
                TaskName = task.TaskName,
                Description = task.Description,
                PriorityNum = task.PriorityNum,
                DueDate = task.DueDate,
                Status = task.Status,
                Session = task.Session,
                CompletedAt = task.CompletedAt,
                CreatedAt = task.CreatedAt,
                AssignedToUser = new UserResponseModel
                {
                    UserId = task.AssignedToUser.Id,
                    FullName = task.AssignedToUser.FullName,
                    Email = task.AssignedToUser.Email,
                    PhoneNumber = task.AssignedToUser.PhoneNumber
                },
                TaskType = task.TaskType == null ? null : new TaskTypeResponseModel
                {
                    TaskTypeId = task.TaskType.Id,
                    TaskTypeName = task.TaskType.TaskTypeName
                },
                StatusLogs = task.StatusLogs.Select(sl => new StatusLogResponseModel
                {

                    Status = sl.Status, // Tên status từ bảng Status
                    UpdatedAt = sl.UpdatedAt
                }).ToList()
            };
        }


        public async Task<bool> UpdateTaskPrioritiesAsync(List<TaskPriorityUpdateModel> taskPriorityUpdates)
        {
            // Kiểm tra đầu vào không null
            if (taskPriorityUpdates == null || !taskPriorityUpdates.Any())
                throw new ArgumentException("The request list cannot be null or empty.");

            // Kiểm tra các điều kiện của request:
            // 1. Không có priorityNum trùng nhau
            // 2. Không có priorityNum nào null
            // 3. Các priorityNum >= 1
            var distinctPriorities = taskPriorityUpdates.Select(t => t.PriorityNum).Distinct().ToList();
            if (distinctPriorities.Count != taskPriorityUpdates.Count)
                throw new ArgumentException("Each priorityNum must be unique in the request.");
            if (taskPriorityUpdates.Any(t => t.PriorityNum < 1))
                throw new ArgumentException("PriorityNum must be greater than or equal to 1.");
            if (taskPriorityUpdates.Any(t => t.PriorityNum == null || t.TaskId == null))
                throw new ArgumentException("PriorityNum cannot be null.");

            // Lấy danh sách taskId từ request
            var taskIds = taskPriorityUpdates.Select(t => t.TaskId).ToList();

            // Lấy các task từ database dựa trên danh sách taskId
            var tasksInDb = await _unitOfWork.Tasks.FindAll()
                .Where(t => taskIds.Contains(t.Id))
                .ToListAsync();

            // Kiểm tra nếu số lượng task trong database không khớp với request
            if (tasksInDb.Count != taskIds.Count)
                throw new ArgumentException("Some tasks in the request do not exist in the database.");

            // Lấy danh sách priorityNum hiện tại trong database
            var dbPriorities = tasksInDb.Select(t => t.PriorityNum).OrderBy(p => p).ToList();

            // Lấy danh sách priorityNum từ request
            var requestPriorities = taskPriorityUpdates.Select(t => t.PriorityNum).OrderBy(p => p).ToList();

            // Kiểm tra nếu danh sách priorityNum không khớp (swap các giá trị)
            if (!dbPriorities.SequenceEqual(requestPriorities))
                throw new ArgumentException("The priorityNum values in the request must match the existing values in the database.");

            // Cập nhật priorityNum cho các task
            foreach (var task in tasksInDb)
            {
                var updateRequest = taskPriorityUpdates.First(t => t.TaskId == task.Id);
                task.PriorityNum = updateRequest.PriorityNum;
            }

            // Lưu thay đổi vào database
            await _unitOfWork.Tasks.UpdateListAsync(tasksInDb);
            await _unitOfWork.CommitAsync();

            return true;
        }

        //Get list task by user
        public async Task<List<SessionTaskGroupModel>> GetUserTasksAsync(Guid userId, DateTime? filterDate = null, Guid? cageId = null)
        {
            // 1. Get all cages assigned to the user
            var userCages = await _unitOfWork.CageStaffs
                .FindByCondition(cs => cs.StaffFarmId == userId)
                .Select(cs => cs.CageId)
                .ToListAsync();

            if (!userCages.Any())
            {
                throw new ArgumentException($"No cages assigned to user with ID {userId}.");
            }

            // 2. Nếu có cageId, chỉ lấy cage đó
            if (cageId.HasValue)
            {
                if (!userCages.Contains(cageId.Value))
                {
                    throw new ArgumentException($"The specified CageId {cageId} is not assigned to the user.");
                }

                userCages = new List<Guid> { cageId.Value };
            }

            // 3. Get all tasks for these cages, apply date filter if provided
            var tasksQuery = _unitOfWork.Tasks
                .FindByCondition(t => userCages.Contains(t.CageId) && t.AssignedToUserId == userId)
                .Include(t => t.TaskType)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Cage)
                //.Include(t => t.StatusLogs)
                //.ThenInclude(x => x.Status)
                .Select(t => new
                {
                    t.Id,
                    t.CageId,
                    t.TaskName,
                    t.Description,
                    t.PriorityNum,
                    t.DueDate,
                    t.Status,
                    t.CompletedAt,
                    t.CreatedAt,
                    t.Session,
                    t.IsTreatmentTask,
                    t.PrescriptionId,
                    CageName = t.Cage.Name, // Use CageName from database
                    AssignedToUser = new
                    {
                        t.AssignedToUser.Id,
                        t.AssignedToUser.FullName,
                        t.AssignedToUser.Email,
                        t.AssignedToUser.PhoneNumber
                    },
                    TaskType = new
                    {
                        t.TaskType.Id,
                        t.TaskType.TaskTypeName
                    }
                });

            // Apply date filter if filterDate is provided
            if (filterDate.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.DueDate.Value.Date == filterDate.Value.Date);
            }
            else
            {
                tasksQuery = tasksQuery.Where(t => t.DueDate.Value.Date == DateTime.Today);
            }

            var tasks = await tasksQuery.ToListAsync();

            // 4. Group tasks by Session → Cage → Tasks
            var groupedTasks = tasks
                .GroupBy(t => t.Session) // Group by session
                .OrderBy(sessionGroup => sessionGroup.Key) // Sort by session key (Morning → Afternoon → Evening)
                .Select(sessionGroup => new SessionTaskGroupModel
                {
                    SessionName = Enum.GetName(typeof(SessionTypeEnum), sessionGroup.Key),
                    Cages = sessionGroup
                        .GroupBy(t => new { t.CageId, t.CageName }) // Group by cage within session
                        .Select(cageGroup => new CageTaskGroupModel
                        {
                            CageId = cageGroup.Key.CageId,
                            CageName = cageGroup.Key.CageName,
                            Tasks = cageGroup.Select(task => new TaskDetailModel
                            {
                                Id = task.Id,
                                TaskName = task.TaskName,
                                Description = task.Description,
                                PriorityNum = task.PriorityNum,
                                DueDate = task.DueDate,
                                Status = task.Status,
                                CompletedAt = task.CompletedAt,
                                CreatedAt = task.CreatedAt,
                                IsTreatmentTask = task.IsTreatmentTask,
                                PrescriptionId = task.PrescriptionId,
                                AssignedToUser = new UserResponseModel
                                {
                                    UserId = task.AssignedToUser.Id,
                                    FullName = task.AssignedToUser.FullName,
                                    Email = task.AssignedToUser.Email,
                                    PhoneNumber = task.AssignedToUser.PhoneNumber
                                },
                                TaskType = new TaskTypeResponseModel
                                {
                                    TaskTypeId = task.TaskType.Id,
                                    TaskTypeName = task.TaskType.TaskTypeName
                                }
                            }).ToList()
                        }).ToList()
                }).ToList();

            return groupedTasks;
        }


        public async Task<bool> UpdateTaskAsync(TaskDetailUpdateModel model)
        {
            // 1. Kiểm tra TaskId có tồn tại hay không
            var task = await _unitOfWork.Tasks.FindByCondition(t => t.Id == model.TaskId).FirstOrDefaultAsync();
            if (task == null)
            {
                throw new ArgumentException($"Task with ID {model.TaskId} does not exist.");
            }

            // 2. Chỉ cho phép cập nhật các task có trạng thái Pending
            if (!string.Equals(task.Status, TaskStatusEnum.Pending, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Only tasks with status 'Pending' can be updated.");
            }

            // 3. Kiểm tra TaskTypeId có giá trị hay không
            Guid? taskTypeIdToCheck = model.TaskTypeId ?? task.TaskTypeId;

            if (taskTypeIdToCheck.HasValue)
            {
                // 3.1 Kiểm tra TaskTypeId có tồn tại không
                var taskType = await _unitOfWork.TaskTypes.FindAsync(x => x.Id == taskTypeIdToCheck);
                if (taskType == null)
                {
                    throw new ArgumentException("Invalid TaskTypeId.");
                }
            }

            // 4. Kiểm tra DueDate có giá trị hay không
            var newDueDate = model.DueDate ?? task.DueDate;

            if (newDueDate.HasValue)
            {
                // 4.1 Kiểm tra ngày hợp lệ (chỉ cho phép hôm nay hoặc ngày mai)
                var currentDate = DateTimeUtils.VietnamNow().Date;
                if (newDueDate.Value.Date < currentDate || newDueDate.Value.Date > currentDate.AddDays(1))
                {
                    throw new InvalidOperationException("You can only update tasks scheduled for today or tomorrow.");
                }
            }
            else
            {
                throw new ArgumentException("DueDate is required.");
            }

            // 5. Kiểm tra Session
            if (model.Session <= 0 || !Enum.IsDefined(typeof(SessionTypeEnum), model.Session))
            {
                throw new ArgumentException("Invalid session value provided.");
            }
            int newSession = model.Session;

            // 6. Kiểm tra trong ngày và session có task nào cùng TaskTypeId hay không
            if (taskTypeIdToCheck.HasValue)
            {
                var duplicateTaskExists = await _unitOfWork.Tasks
                    .FindByCondition(t => t.DueDate.HasValue &&
                                          t.DueDate.Value.Date == newDueDate.Value.Date &&
                                          t.Session == newSession &&
                                          t.CageId == task.CageId &&
                                          t.TaskTypeId == taskTypeIdToCheck &&
                                          t.Id != task.Id)
                    .AnyAsync();

                if (duplicateTaskExists)
                {
                    throw new InvalidOperationException($"A task of type {taskTypeIdToCheck} already exists in cage {task.CageId} on {newDueDate.Value.Date.ToShortDateString()} during session {newSession}.");
                }
            }

            // 7. Cập nhật các trường còn lại
            task.DueDate = newDueDate;
            task.Session = newSession;

            if (!string.IsNullOrEmpty(model.TaskName))
            {
                // Kiểm tra trùng tên trong cùng ngày và session
                var duplicateTaskNameExists = await _unitOfWork.Tasks
                    .FindByCondition(t => t.DueDate.HasValue &&
                                          t.DueDate.Value.Date == task.DueDate.Value.Date &&
                                          t.Session == task.Session &&
                                          t.CageId == task.CageId &&
                                          t.TaskName == model.TaskName &&
                                          t.Id != task.Id)
                    .AnyAsync();

                if (duplicateTaskNameExists)
                {
                    throw new InvalidOperationException($"A task with the name '{model.TaskName}' already exists in cage {task.CageId} during session {task.Session}.");
                }

                task.TaskName = model.TaskName;
            }

            if (!string.IsNullOrEmpty(model.Description))
            {
                task.Description = model.Description;
            }

            // 8. Lưu thay đổi
            await _unitOfWork.Tasks.UpdateAsync(task);
            await _unitOfWork.CommitAsync();

            return true;
        }



        public async Task<bool> GenerateTasksForTodayAsync()
        {
            var today = DateTimeUtils.VietnamNow().Date;

            // Lấy tất cả các FarmingBatch đang ở trạng thái Active
            var activeBatches = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.Status == FarmingBatchStatusEnum.Active)
                .Include(fb => fb.GrowthStages)
                .ThenInclude(gs => gs.TaskDailies)
                .Include(fb => fb.GrowthStages)
                .ThenInclude(gs => gs.VaccineSchedules)
                .Include(fb => fb.Cage)
                .ToListAsync();

            var tasksToCreate = new List<Task>();

            foreach (var batch in activeBatches)
            {
                // Lấy AdminId của Farm liên quan đến Cage
                var adminId = await GetFarmAdminIdByCageId(batch.CageId);

                foreach (var growthStage in batch.GrowthStages)
                {
                    // Kiểm tra nếu ngày hôm nay nằm trong khoảng thời gian của GrowthStage
                    if (growthStage.AgeStartDate <= today && growthStage.AgeEndDate >= today)
                    {
                        // Generate Task từ TaskDaily
                        foreach (var taskDaily in growthStage.TaskDailies)
                        {
                            var existingTask = await _unitOfWork.Tasks
                                .FindByCondition(t =>
                                    t.CageId == batch.CageId &&
                                    t.TaskName == taskDaily.TaskName &&
                                    t.DueDate == today &&
                                    t.Session == taskDaily.Session)
                                .AnyAsync();

                            if (!existingTask)
                            {
                                var assignedStaff = await GetAssignedStaffForCage(batch.CageId, today);

                                if (assignedStaff == null)
                                {
                                    throw new Exception($"No staff assigned for Cage {batch.CageId} on {today}.");
                                }

                                tasksToCreate.Add(new Task
                                {
                                    TaskTypeId = taskDaily.TaskTypeId,
                                    CageId = batch.CageId,
                                    AssignedToUserId = (Guid)assignedStaff,
                                    CreatedByUserId = (Guid)adminId,
                                    TaskName = taskDaily.TaskName,
                                    PriorityNum = taskDaily.TaskTypeId.HasValue
                                        ? (await _unitOfWork.TaskTypes.FindByCondition(tt => tt.Id == taskDaily.TaskTypeId).FirstOrDefaultAsync())?.PriorityNum ?? 0
                                        : 0,
                                    Description = taskDaily.Description,
                                    DueDate = today,
                                    Session = taskDaily.Session,
                                    Status = TaskStatusEnum.Pending,
                                    CreatedAt = DateTimeUtils.VietnamNow()
                                });
                            }
                        }

                        // Generate Task từ VaccineSchedule
                        foreach (var vaccineSchedule in growthStage.VaccineSchedules)
                        {
                            if (vaccineSchedule.Date == today && vaccineSchedule.Status == VaccineScheduleStatusEnum.Upcoming)
                            {
                                var assignedStaff = await GetAssignedStaffForCage(batch.CageId, today);

                                if (assignedStaff == null)
                                {
                                    throw new Exception($"No staff assigned for Cage {batch.CageId} on {today}.");
                                }

                                var vaccineTaskTypeId = await GetTaskTypeIdByName("Tiêm vắc xin");
                                var vaccineTaskType = await _unitOfWork.TaskTypes.FindByCondition(tt => tt.Id == vaccineTaskTypeId).FirstOrDefaultAsync();

                                tasksToCreate.Add(new Task
                                {
                                    TaskTypeId = vaccineTaskTypeId,
                                    CageId = batch.CageId,
                                    AssignedToUserId = (Guid)assignedStaff,
                                    CreatedByUserId = (Guid)adminId,
                                    TaskName = $"Tiêm vắc xin: {vaccineSchedule.Vaccine.Name}",
                                    PriorityNum = (int)vaccineTaskType?.PriorityNum,
                                    Description = $"Tiêm vắc xin {vaccineSchedule.Vaccine.Name} cho giai đoạn {growthStage.Name}.",
                                    DueDate = today,
                                    Session = 1,
                                    Status = TaskStatusEnum.Pending,
                                    CreatedAt = DateTimeUtils.VietnamNow()
                                });

                                // Cập nhật trạng thái của VaccineSchedule
                                vaccineSchedule.Status = VaccineScheduleStatusEnum.Completed;
                                await _unitOfWork.VaccineSchedules.UpdateAsync(vaccineSchedule);
                            }
                        }
                    }
                }

                // Generate Task dọn dẹp (Cleaning Task)
                if (batch.CleaningFrequency > 0 && (today - batch.StartDate.Value.Date).Days % batch.CleaningFrequency == 0)
                {
                    var assignedStaff = await GetAssignedStaffForCage(batch.CageId, today);

                    if (assignedStaff == null)
                    {
                        throw new Exception($"No staff assigned for Cage {batch.CageId} on {today}.");
                    }

                    var cleaningTaskTypeId = await GetTaskTypeIdByName("Dọn chuồng");

                    tasksToCreate.Add(new Task
                    {
                        TaskTypeId = cleaningTaskTypeId,
                        CageId = batch.CageId,
                        AssignedToUserId = (Guid)assignedStaff,
                        CreatedByUserId = (Guid)adminId,
                        TaskName = "Dọn dẹp chuồng",
                        PriorityNum = cleaningTaskTypeId.HasValue
                            ? (await _unitOfWork.TaskTypes.FindByCondition(tt => tt.Id == cleaningTaskTypeId).FirstOrDefaultAsync())?.PriorityNum ?? 1
                            : 1,
                        Description = "Task dọn dẹp chuồng theo lịch định kỳ.",
                        DueDate = today,    
                        Session = 1,
                        Status = TaskStatusEnum.Pending,
                        CreatedAt = DateTimeUtils.VietnamNow()
                    });
                }
            }

            // Lưu các task vào database
            if (tasksToCreate.Any())
            {
                await _unitOfWork.Tasks.CreateListAsync(tasksToCreate);
                await _unitOfWork.CommitAsync();
            }

            return true;
        }

        public async Task<bool> GenerateTasksForTomorrowAsync()
        {
            // Lấy ngày hôm nay theo giờ Việt Nam
            var today = DateTimeUtils.VietnamNow().Date;

            // Lấy ngày cần generate task (ngày mai)
            var targetDate = today.AddDays(1);
            //var targetDate = today;
            // Lấy tất cả các FarmingBatch đang ở trạng thái Active
            var activeBatches = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.Status == FarmingBatchStatusEnum.Active)
                .Include(fb => fb.GrowthStages)
                .ThenInclude(gs => gs.TaskDailies)
                .Include(fb => fb.GrowthStages)
                .ThenInclude(gs => gs.VaccineSchedules)
                .Include(fb => fb.Cage)
                .ToListAsync();

            var tasksToCreate = new List<Task>();

            foreach (var batch in activeBatches)
            {
                var existingTasksForTargetDateaa = await _unitOfWork.Tasks
                    .FindByCondition(t => t.DueDate.Value.Date == targetDate.Date && t.CageId == batch.CageId).ToListAsync();
                var existingTasksForTargetDate = await _unitOfWork.Tasks
                    .FindByCondition(t => t.DueDate.Value.Date == targetDate.Date && t.CageId == batch.CageId)
                    .AnyAsync();

                if (existingTasksForTargetDate)
                {
                    continue; // Nếu đã có task cho ngày này, bỏ qua
                }

                // Lấy AdminId của Farm liên quan đến Cage
                var adminId = await GetFarmAdminIdByCageId(batch.CageId);

                foreach (var growthStage in batch.GrowthStages)
                {
                    // Kiểm tra nếu ngày cần generate nằm trong khoảng thời gian của GrowthStage
                    if (growthStage.AgeStartDate.Value.Date <= targetDate.Date && growthStage.AgeEndDate.Value.Date >= targetDate.Date)
                    {
                        // Generate Task từ TaskDaily
                        foreach (var taskDaily in growthStage.TaskDailies)
                        {
                            tasksToCreate.Add(new Task
                            {
                                TaskTypeId = taskDaily.TaskTypeId,
                                CageId = batch.CageId,
                                AssignedToUserId = (await GetAssignedStaffForCage(batch.CageId, targetDate)) ?? Guid.Empty,
                                CreatedByUserId = (Guid)adminId,
                                TaskName = taskDaily.TaskName,
                                PriorityNum = taskDaily.TaskTypeId.HasValue
                                    ? (await _unitOfWork.TaskTypes.FindByCondition(tt => tt.Id == taskDaily.TaskTypeId).FirstOrDefaultAsync())?.PriorityNum ?? 0
                                    : 0,
                                Description = taskDaily.Description,
                                DueDate = targetDate,
                                Session = taskDaily.Session,
                                Status = TaskStatusEnum.Pending,
                                CreatedAt = DateTimeUtils.VietnamNow()
                            });
                        }

                        // Generate Task từ VaccineSchedule
                        foreach (var vaccineSchedule in growthStage.VaccineSchedules)
                        {
                            if (vaccineSchedule.Date == targetDate && vaccineSchedule.Status == VaccineScheduleStatusEnum.Upcoming)
                            {
                                tasksToCreate.Add(new Task
                                {
                                    TaskTypeId = await GetTaskTypeIdByName("Tiêm vắc xin"),
                                    CageId = batch.CageId,
                                    AssignedToUserId = (await GetAssignedStaffForCage(batch.CageId, targetDate)) ?? Guid.Empty,
                                    CreatedByUserId = (Guid)adminId,
                                    TaskName = $"Tiêm vắc xin: {vaccineSchedule.Vaccine.Name}",
                                    PriorityNum = 2,
                                    Description = $"Tiêm vắc xin {vaccineSchedule.Vaccine.Name} cho giai đoạn {growthStage.Name}.",
                                    DueDate = targetDate,
                                    Session = vaccineSchedule.Session,
                                    Status = TaskStatusEnum.Pending,
                                    CreatedAt = DateTimeUtils.VietnamNow()
                                });

                                vaccineSchedule.Status = VaccineScheduleStatusEnum.Completed;
                                await _unitOfWork.VaccineSchedules.UpdateAsync(vaccineSchedule);
                            }
                        }
                    }
                }
            }

            // Lưu các task vào database
            if (tasksToCreate.Any())
            {
                await _unitOfWork.Tasks.CreateListAsync(tasksToCreate);
                await _unitOfWork.CommitAsync();
            }

            return true;
        }



        private async Task<Guid?> GetAssignedStaffForCage(Guid cageId, DateTime date)
        {
            // 1. Lấy thông tin nhân viên mặc định của chuồng
            var defaultStaff = await _unitOfWork.CageStaffs
                .FindByCondition(cs => cs.CageId == cageId)
                .FirstOrDefaultAsync();

            if (defaultStaff == null)
            {
                // Không có nhân viên mặc định gán cho chuồng
                return null;
            }

            // 2. Kiểm tra xem nhân viên mặc định có đơn nghỉ phép hợp lệ
            var leaveRequest = await _unitOfWork.LeaveRequests
                .FindByCondition(lr => lr.StaffFarmId == defaultStaff.StaffFarmId &&
                                       lr.StartDate <= date &&
                                       lr.EndDate >= date &&
                                       lr.Status == "Approved")
                .FirstOrDefaultAsync();

            if (leaveRequest != null)
            {
                // Nếu có đơn nghỉ phép đã được phê duyệt, trả về UserTempId
                return leaveRequest.UserTempId;
            }

            // 3. Nếu không có đơn nghỉ phép, trả về nhân viên mặc định
            return defaultStaff.StaffFarmId;
        }

        private async Task<Guid?> GetFarmAdminIdByCageId(Guid cageId)
        {
            // Tìm Cage để lấy FarmId
            var cage = await _unitOfWork.Cages
                .FindByCondition(c => c.Id == cageId)
                .FirstOrDefaultAsync();

            if (cage == null)
            {
                throw new Exception($"Cage with ID {cageId} does not exist.");
            }

            // Tìm AdminId từ bảng FarmAdmin dựa trên FarmId
            var farmAdmin = await _unitOfWork.FarmsAdmins
                .FindByCondition(fa => fa.FarmId == cage.FarmId)
                .FirstOrDefaultAsync();

            return farmAdmin?.AdminId;
        }


        private async Task<Guid?> GetTaskTypeIdByName(string taskTypeName)
        {
            var taskType = await _unitOfWork.TaskTypes
                .FindByCondition(tt => tt.TaskTypeName == taskTypeName)
                .FirstOrDefaultAsync();

            return taskType?.Id;
        }

        public async Task<bool> UpdateAllTaskStatusesAsync()
        {
            var today = DateTimeUtils.VietnamNow().Date; // Lấy ngày hôm nay
            var currentTime = DateTimeUtils.VietnamNow().TimeOfDay; // Lấy giờ hiện tại

            // Lấy session hiện tại
            var currentSession = SessionTime.GetCurrentSession(currentTime);

            if (currentSession == -1)
            {
                throw new Exception("Current time is not within any defined session.");
            }

            // Lấy tất cả các task trong ngày hôm nay
            var tasks = await _unitOfWork.Tasks
                .FindByCondition(t => t.DueDate.Value.Date == today.Date)
                .ToListAsync();

            foreach (var task in tasks)
            {
                if (task.Session < currentSession) // Session đã qua
                {
                    if (task.Status == TaskStatusEnum.Pending || task.Status == TaskStatusEnum.InProgress)
                    {
                        task.Status = TaskStatusEnum.Overdue; // Chuyển sang OverSchedules
                        await _unitOfWork.Tasks.UpdateAsync(task);
                    }
                }
                else if (task.Session == currentSession) // Session hiện tại
                {
                    if (task.Status == TaskStatusEnum.Pending)
                    {
                        task.Status = TaskStatusEnum.InProgress; // Chuyển sang InProgress
                        await _unitOfWork.Tasks.UpdateAsync(task);
                    }
                }
            }


            await _unitOfWork.CommitAsync();


            return true;
        }
        public async Task<bool> GenerateTasksForFarmingBatchAsync(Guid farmingBatchId)
        {
            var today = DateTimeUtils.VietnamNow().Date;
            var currentTime = DateTimeUtils.VietnamNow().TimeOfDay;
            var tomorrow = today.AddDays(1);

            // Lấy FarmingBatch
            var farmingBatch = await _unitOfWork.FarmingBatches
                .FindByCondition(fb => fb.Id == farmingBatchId && fb.Status == FarmingBatchStatusEnum.Active)
                .Include(fb => fb.GrowthStages)
                .ThenInclude(gs => gs.TaskDailies)
                .Include(fb => fb.GrowthStages)
                .ThenInclude(gs => gs.VaccineSchedules)
                .Include(fb => fb.Cage)
                .FirstOrDefaultAsync();

            if (farmingBatch == null)
            {
                throw new ArgumentException($"FarmingBatch with ID {farmingBatchId} is not active or does not exist.");
            }

            var tasksToCreate = new List<Task>();

            // Generate tasks for today
            tasksToCreate.AddRange(await GenerateTasksForDate(farmingBatch, today, currentTime));

            // Generate tasks for tomorrow
            tasksToCreate.AddRange(await GenerateTasksForDate(farmingBatch, tomorrow, null)); // Không cần kiểm tra thời gian

            // Lưu các task vào database
            if (tasksToCreate.Any())
            {
                await _unitOfWork.Tasks.CreateListAsync(tasksToCreate);
                await _unitOfWork.CommitAsync();
            }

            return true;
        }


        private async Task<List<Task>> GenerateTasksForDate(FarmingBatch farmingBatch, DateTime date, TimeSpan? currentTime)
        {
            var tasks = new List<Task>();
            var adminId = await GetFarmAdminIdByCageId(farmingBatch.CageId);

            foreach (var growthStage in farmingBatch.GrowthStages)
            {
                // Kiểm tra nếu ngày nằm trong khoảng thời gian của GrowthStage
                if (growthStage.AgeStartDate.Value.Date <= date.Date && growthStage.AgeEndDate.Value >= date.Date)
                {
                    var currentSession = currentTime.HasValue ? SessionTime.GetCurrentSession(currentTime.Value) : 1;

                    // Generate Task từ TaskDaily
                    foreach (var taskDaily in growthStage.TaskDailies)
                    {
                        if (currentTime.HasValue && taskDaily.Session < currentSession)
                        {
                            continue; // Bỏ qua các session đã qua nếu là ngày hôm nay
                        }

                        var existingTask = await _unitOfWork.Tasks
                            .FindByCondition(t =>
                                t.CageId == farmingBatch.CageId &&
                                t.TaskName == taskDaily.TaskName &&
                                t.DueDate.Value.Date == date &&
                                t.Session == taskDaily.Session)
                            .AnyAsync();

                        if (!existingTask)
                        {
                            var assignedStaff = await GetAssignedStaffForCage(farmingBatch.CageId, date);

                            if (assignedStaff == null)
                            {
                                throw new Exception($"No staff assigned for Cage {farmingBatch.CageId} on {date}.");
                            }

                            var sessionEndTime = GetSessionEndTime(taskDaily.Session);

                            tasks.Add(new Task
                            {
                                TaskTypeId = taskDaily.TaskTypeId,
                                CageId = farmingBatch.CageId,
                                AssignedToUserId = (Guid)assignedStaff,
                                CreatedByUserId = (Guid)adminId,
                                TaskName = taskDaily.TaskName,
                                PriorityNum = taskDaily.TaskTypeId.HasValue
                                    ? (await _unitOfWork.TaskTypes.FindByCondition(tt => tt.Id == taskDaily.TaskTypeId).FirstOrDefaultAsync())?.PriorityNum ?? 0
                                    : 0,
                                Description = taskDaily.Description,
                                DueDate = date.Add(sessionEndTime),
                                Session = taskDaily.Session,
                                Status = TaskStatusEnum.Pending,
                                CreatedAt = DateTimeUtils.VietnamNow()
                            });
                        }
                    }

                    // Generate Task từ VaccineSchedule
                    foreach (var vaccineSchedule in growthStage.VaccineSchedules)
                    {
                        if (vaccineSchedule.Date == date && vaccineSchedule.Status == VaccineScheduleStatusEnum.Upcoming)
                        {
                            var assignedStaff = await GetAssignedStaffForCage(farmingBatch.CageId, date);

                            if (assignedStaff == null)
                            {
                                throw new Exception($"No staff assigned for Cage {farmingBatch.CageId} on {date}.");
                            }

                            var vaccineTaskTypeId = await GetTaskTypeIdByName("Tiêm vắc xin");
                            var vaccineTaskType = await _unitOfWork.TaskTypes.FindByCondition(tt => tt.Id == vaccineTaskTypeId).FirstOrDefaultAsync();

                            tasks.Add(new Task
                            {
                                TaskTypeId = vaccineTaskTypeId,
                                CageId = farmingBatch.CageId,
                                AssignedToUserId = (Guid)assignedStaff,
                                CreatedByUserId = (Guid)adminId,
                                TaskName = $"Tiêm vắc xin: {vaccineSchedule.Vaccine.Name}",
                                PriorityNum = (int)vaccineTaskType?.PriorityNum,
                                Description = $"Tiêm vắc xin {vaccineSchedule.Vaccine.Name} cho giai đoạn {growthStage.Name}.",
                                DueDate = date.Add(GetSessionEndTime(1)),
                                Session = vaccineSchedule.Session,
                                Status = TaskStatusEnum.Pending,
                                CreatedAt = DateTimeUtils.VietnamNow()
                            });

                            // Cập nhật trạng thái của VaccineSchedule
                            vaccineSchedule.Status = VaccineScheduleStatusEnum.Completed;
                            await _unitOfWork.VaccineSchedules.UpdateAsync(vaccineSchedule);
                        }
                    }
                }
            }

            // Generate Cleaning Task
            if (farmingBatch.CleaningFrequency > 0 && (date - farmingBatch.StartDate.Value.Date).Days % farmingBatch.CleaningFrequency == 0)
            {
                var assignedStaff = await GetAssignedStaffForCage(farmingBatch.CageId, date);

                if (assignedStaff == null)
                {
                    throw new Exception($"No staff assigned for Cage {farmingBatch.CageId} on {date}.");
                }

                var cleaningTaskTypeId = await GetTaskTypeIdByName("Dọn chuồng");

                tasks.Add(new Task
                {
                    TaskTypeId = cleaningTaskTypeId,
                    CageId = farmingBatch.CageId,
                    AssignedToUserId = (Guid)assignedStaff,
                    CreatedByUserId = (Guid)adminId,
                    TaskName = "Dọn dẹp chuồng",
                    PriorityNum = cleaningTaskTypeId.HasValue
                        ? (await _unitOfWork.TaskTypes.FindByCondition(tt => tt.Id == cleaningTaskTypeId).FirstOrDefaultAsync())?.PriorityNum ?? 1
                        : 1,
                    Description = "Task dọn dẹp chuồng theo lịch định kỳ.",
                    DueDate = date.Add(GetSessionEndTime(1)),
                    Session = 1,
                    Status = TaskStatusEnum.Pending,
                    CreatedAt = DateTimeUtils.VietnamNow()
                });
            }

            return tasks;
        }


        private TimeSpan GetSessionEndTime(int session)
        {
            return session switch
            {
                1 => SessionTime.Morning.End, // Giờ kết thúc buổi sáng
                2 => SessionTime.Noon.End, // Giờ kết thúc buổi chiều
                3 => SessionTime.Afternoon.End, // Giờ kết thúc buổi tối
                4 => SessionTime.Evening.End,
                _ => throw new ArgumentException($"Invalid session value '{session}'.")
            };
        }
        public async Task<bool> UpdateEveningTaskStatusesAsync()
        {
            var today = DateTimeUtils.VietnamNow().Date; // Lấy ngày hôm nay
            var tasks = await _unitOfWork.Tasks
                .FindByCondition(t => t.DueDate == today && t.Session == 3)
                .ToListAsync();

            foreach (var task in tasks)
            {
                if (task.Status == TaskStatusEnum.Pending || task.Status == TaskStatusEnum.InProgress)
                {
                    task.Status = TaskStatusEnum.Overdue;
                    await _unitOfWork.Tasks.UpdateAsync(task);
                }
            }

            await _unitOfWork.CommitAsync();
            return true;
        }



    }
}
