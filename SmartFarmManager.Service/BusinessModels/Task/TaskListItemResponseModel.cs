using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class TaskListItemResponseModel
    {
        public Guid Id { get; set; }
        public Guid? TaskTypeId { get; set; }

        public Guid CageId { get; set; }

        public Guid AssignedToUserId { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; }
        public int Session { get;set; }
        public DateTime? CompletedAt { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
