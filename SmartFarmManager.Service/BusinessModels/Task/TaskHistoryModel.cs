using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Task
{
    public class TaskHistoryModel
    {
        public int Id { get; set; }
        public string? StatusBefore { get; set; }
        public string StatusAfter { get; set; } = null!;
        public DateTime ChangedAt { get; set; }
    }
}
