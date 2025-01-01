using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Helpers
{
    public static class SessionTime
    {
        public static readonly (TimeSpan Start, TimeSpan End) Morning = (TimeSpan.FromHours(6), TimeSpan.FromHours(11));  // 6:00 - 11:59
        public static readonly (TimeSpan Start, TimeSpan End) Afternoon = (TimeSpan.FromHours(12), TimeSpan.FromHours(17));  // 12:00 - 17:59
        public static readonly (TimeSpan Start, TimeSpan End) Evening = (TimeSpan.FromHours(18), TimeSpan.FromHours(23));  // 18:00 - 22:59

        public static int GetCurrentSession(TimeSpan currentTime)
        {
            if (currentTime >= Morning.Start && currentTime < Morning.End)
            {
                return 1; // Morning
            }
            if (currentTime >= Afternoon.Start && currentTime < Afternoon.End)
            {
                return 2; // Afternoon
            }
            if (currentTime >= Evening.Start && currentTime < Evening.End)
            {
                return 3; // Evening
            }

            return -1; // Invalid session
        }
    }

}
