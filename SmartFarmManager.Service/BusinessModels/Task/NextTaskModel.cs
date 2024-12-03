using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class NextTaskModel
    {
        public Guid TaskId { get; set; }
        public string TaskName { get; set; }
        public string Cagename { get; set; }
        public string AssignName { get; set; }
        public int PriorityNum { get; set; }
        public string Status { get; set; }
        public DateTime? DueDate { get; set; }
        public int Total { get; set; }
        public int TaskDone { get; set; }
    }
}
