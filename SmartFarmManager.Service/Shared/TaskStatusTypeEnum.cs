using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Shared
{
    public static class TaskStatusTypeEnum
    {
        public const string Pending = "Pending";
        public const string InProgress = "InProgress";
        public const string Done = "Done";
        public const string OverSchedules = "OverSchedules";
    }
}
