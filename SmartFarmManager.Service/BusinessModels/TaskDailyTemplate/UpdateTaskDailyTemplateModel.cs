using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.TaskDailyTemplate
{
    public class UpdateTaskDailyTemplateModel
    {
        public Guid Id { get; set; }
        public Guid? TaskTypeId { get; set; }
        public string? TaskName { get; set; }
        public string? Description { get; set; }
        public int? Session { get; set; }
    }

}
