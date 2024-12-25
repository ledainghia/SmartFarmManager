using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Shared
{
    public static class FarmingBatchStatusEnum
    {
        public const string Planning = "Planning";
        public const string Active = "Đang diễn ra";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }

}
