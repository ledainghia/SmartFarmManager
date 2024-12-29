using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.TaskDaily
{
    public class TaskDailyModel
    {
        public Guid Id { get; set; }
        public Guid GrowthStageId { get; set; }
        public Guid? TaskTypeId { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public int Session { get; set; }
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }
    }
}
