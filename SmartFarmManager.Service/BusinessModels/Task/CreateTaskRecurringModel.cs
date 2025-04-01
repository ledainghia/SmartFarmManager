using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class CreateTaskRecurringModel
    {
        public Guid CageId { get; set; }
        public Guid TaskTypeId { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public List<int> Sessions { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }
}
