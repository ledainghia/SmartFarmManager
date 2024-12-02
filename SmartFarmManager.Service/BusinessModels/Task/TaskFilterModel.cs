using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class TaskFilterModel
    {
        public string? TaskName { get; set; }
        public string? Status { get; set; }
        public Guid? TaskTypeId { get; set; }
        public Guid? CageId { get; set; }
        public Guid? AssignedToUserId { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public int? PriorityNum { get; set; }
        public int? Session { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}

