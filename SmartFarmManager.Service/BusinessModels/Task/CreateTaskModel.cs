using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class CreateTaskModel
    {
        public string TaskName { get; set; }
        public DateTime DueDate { get; set; }
        public string? Description { get; set; }
        public string TaskType { get; set; }
        public int? FarmId { get; set; }
        public int? AssignedToUserId { get; set; }
        public int CreatedBy { get; set; } // ID của người tạo
    }
}
