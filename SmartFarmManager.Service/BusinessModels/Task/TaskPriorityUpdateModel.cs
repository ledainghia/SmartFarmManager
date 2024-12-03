using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class TaskPriorityUpdateModel
    {
        public Guid TaskId { get; set; }
        public int PriorityNum { get; set; }
    }
}
