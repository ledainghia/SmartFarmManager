using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFarmManager.DataAccessObject.Models;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class TaskModel
    {
        public Guid? TaskTypeId { get; set; }

        public Guid? CageId { get; set; }

        public Guid? AssignedToUserId { get; set; }

        public Guid? CreatedByUserId { get; set; }

        public string? TaskName { get; set; }

        public int? PriorityNum { get; set; }

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public string? Status { get; set; }
        public int? Session { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? CreatedAt { get; set; }

        public virtual User AssignedToUser { get; set; }

        public virtual Cage Cage { get; set; }

        public virtual ICollection<StatusLog> StatusLogs { get; set; } = new List<StatusLog>();

        //public virtual TaskType TaskType { get; set; }
    }
}
