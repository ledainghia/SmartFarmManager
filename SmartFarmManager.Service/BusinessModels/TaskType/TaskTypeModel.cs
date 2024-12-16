using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.TaskType
{
    public class TaskTypeModel
    {
        public Guid Id { get; set; }
        public string TaskTypeName { get; set; }
        public int? PriorityNum { get; set; }
    }
}
