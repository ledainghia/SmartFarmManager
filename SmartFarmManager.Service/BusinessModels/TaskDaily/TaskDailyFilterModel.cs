using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.TaskDaily
{
    public class TaskDailyFilterModel
    {
        public Guid GrowthStageId { get; set; }
        public string? TaskName { get; set; }
        public int? Session { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

}
