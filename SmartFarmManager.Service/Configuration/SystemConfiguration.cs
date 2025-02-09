using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Configuration
{
    public class SystemConfiguration
    {
        public int MaxFarmingBatchPerCage { get; set; } = 1;
        public int FirstReminderTimeHours { get; set; } = 6;  // Thời gian nhắc nhở lần đầu (sau 6 giờ)
        public int SecondReminderTimeHours { get; set; } = 12; // Thời gian nhắc nhở lần 2 (sau 12 giờ)
    }
}
