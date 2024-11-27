using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class UpdateTaskPriorityModel
    {
        public int NewPriority { get; set; }
        public int Session { get; set; }
    }
}
