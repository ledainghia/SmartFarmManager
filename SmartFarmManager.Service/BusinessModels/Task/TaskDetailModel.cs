using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class TaskDetailModel
    {
        public Guid Id { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public int PriorityNum { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public int Session { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }

        public UserResponseModel AssignedToUser { get; set; }

        // Liên kết với TaskType
        public TaskTypeResponseModel TaskType { get; set; }

        // Liên kết với StatusLogs
        public List<StatusLogResponseModel> StatusLogs { get; set; }
    }

    public class UserResponseModel
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class TaskTypeResponseModel
    {
        public Guid TaskTypeId { get; set; }
        public string TaskTypeName { get; set; }
    }

    public class StatusLogResponseModel
    {
        public Guid StatusId { get; set; }
        public string StatusName { get; set; } // Map từ bảng `Status`
        public DateTime? UpdatedAt { get; set; }
    }
}
