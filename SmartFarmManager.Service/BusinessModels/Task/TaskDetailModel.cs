using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class TaskDetailModel
    {
        public int Id { get; set; }
        public string TaskName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime DueDate { get; set; }
        public string TaskType { get; set; } = null!;
        public int? FarmId { get; set; }
        public int? AssignedToUserId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? CompleteAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public List<TaskHistoryModel> TaskHistories { get; set; } = new List<TaskHistoryModel>();
    }
}
