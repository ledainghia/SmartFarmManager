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

namespace SmartFarmManager.Service.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TaskService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<bool> CreateTaskAsync(CreateTaskModel model)
        {
            // Kiểm tra ngày hạn không được nhỏ hơn thời điểm hiện tại
            if (model.DueDate < DateTime.UtcNow)
            {
                throw new ArgumentException("DueDate must be in the future");
            }

            // Kiểm tra TaskTypeId hợp lệ
            var taskType = await _unitOfWork.TaskTypes.FindAsync(x => x.Id == model.TaskTypeId);
            if (taskType == null)
            {
                throw new ArgumentException("Invalid TaskTypeId");
            }

            // Lấy danh sách task cùng ngày, session, cage, và priority
            var existingTask = await _unitOfWork.Tasks
                .FindByCondition(t => t.DueDate.HasValue &&
                                      t.DueDate.Value.Date == model.DueDate.Date &&
                                      t.Session == model.Session &&
                                      t.CageId == model.CageId &&
                                      t.PriorityNum == taskType.PriorityNum &&
                                      t.CompletedAt==null) 
                .FirstOrDefaultAsync();

            // Nếu đã tồn tại task trùng PriorityNum thì trả về lỗi
            if (existingTask != null)
            {
                throw new InvalidOperationException($"A task with priority {taskType.PriorityNum} already exists in cage {model.CageId} on {model.DueDate.Date.ToShortDateString()} during session {model.Session}.");
            }

            // Tạo mới task
            var task = new DataAccessObject.Models.Task
            {
                TaskTypeId = model.TaskTypeId,
                CageId = model.CageId,
                AssignedToUserId = model.AssignedToUserId,
                CreatedByUserId = model.CreatedByUserId,
                TaskName = model.TaskName,
                PriorityNum =(int)taskType.PriorityNum,
                Description = model.Description,
                DueDate = model.DueDate,
                Session = model.Session,
                CreatedAt = DateTime.UtcNow,
                Status = "Assigned"
            };

            await _unitOfWork.Tasks.CreateAsync(task);
            await _unitOfWork.CommitAsync();

            return true;

        }

        public async Task<bool> UpdateTaskPriorityAsync(Guid taskId, UpdateTaskPriorityModel model)
        {
            
            var task = await _unitOfWork.Tasks.FindAsync(x=>x.Id==taskId);
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
        public async Task<bool> ChangeTaskStatusAsync(Guid taskId, Guid statusId)
        {
            var task = await _unitOfWork.Tasks.FindAsync(x => x.Id == taskId);
            if (task == null)
            {
                throw new ArgumentException("Invalid TaskId");
            }

            var status = await _unitOfWork.Statuses.FindAsync(x => x.Id == statusId);
            if (status == null)
            {
                throw new ArgumentException("Invalid StatusId");
            }
            var statusLog = new StatusLog
            {
                TaskId = task.Id,
                StatusId = status.Id,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.StatusLogs.CreateAsync(statusLog);
            if (status.StatusName == "Done")
            {
                task.CompletedAt = DateTime.UtcNow;
            }
            task.Status = status.StatusName;
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

        public async Task<List<TaskResponse>> GetTasksForUserWithStateAsync(Guid userId,Guid cageId, DateTime? specificDate = null)
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
                .Where(t => t.AssignedToUserId == userId && t.DueDate.HasValue && t.DueDate.Value.Date == today)
                .ToListAsync();

            // Tính tổng số task và số task đã hoàn thành
            var totalTasks = userTasksToday.Count;
            var completedTasks = userTasksToday.Count(t => t.CompletedAt != null);

            // Lấy task tiếp theo của từng CageId
            var nextTasks = userTasksToday
                .Where(t => t.CompletedAt == null) // Chỉ lấy task chưa hoàn thành
                .GroupBy(t => t.CageId) // Nhóm theo CageId
                .Select(g => g.OrderBy(t => t.PriorityNum).ThenBy(t => t.DueDate).FirstOrDefault()) // Lấy task ưu tiên cao nhất
                .ToList();

            // Trả về danh sách NextTaskModel
            var result = nextTasks.Select(nextTask => new NextTaskModel
            {
                TaskId = nextTask.Id,
                TaskName = nextTask.TaskName,
                Cagename = nextTask.Cage?.Name ?? "Unknown Cage", // Nếu Cage có tên
                AssignName = nextTask.AssignedToUser?.FullName ?? "Unknown User", // Nếu user có tên
                PriorityNum = nextTask.PriorityNum,
                Status = nextTask.Status,
                DueDate = nextTask.DueDate,
                Total = totalTasks, // Tổng số task của user hôm đó
                TaskDone = completedTasks // Số task đã hoàn thành
            }).ToList();

            return result;
        }



        public async Task<PagedResult<TaskDetailModel>> GetFilteredTasksAsync(TaskFilterModel filter)
        {
            // Query từ repository
            var query = _unitOfWork.Tasks.FindAll(false, x => x.AssignedToUser, x => x.TaskType, x => x.StatusLogs).Include(x=>x.StatusLogs).ThenInclude(x=>x.Status).AsQueryable();

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
                        StatusId = s.StatusId,
                        StatusName = s.Status.StatusName,
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
            .FindByCondition(t => t.Id == taskId, false, x => x.AssignedToUser, x => x.TaskType, x => x.StatusLogs,x=>x.StatusLogs.Select(sl=>sl.Status)).FirstOrDefaultAsync();
            if (task == null)
            {
                return null; 
            }

            // Map Task sang TaskDetailResponse
            return new TaskDetailModel
            {
                Id = task.Id,
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
                    StatusId = sl.StatusId,
                    StatusName = sl.Status.StatusName, // Tên status từ bảng Status
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

    }
}
