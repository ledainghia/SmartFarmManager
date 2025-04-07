﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Helpers
{
    public static class SessionTime
    {
        public static readonly (TimeSpan Start, TimeSpan End) Morning = (TimeSpan.FromHours(6), TimeSpan.FromHours(12));  // 6:00 - 11:59
        public static readonly (TimeSpan Start, TimeSpan End) Noon = (TimeSpan.FromHours(12), TimeSpan.FromHours(14));      // 12:00 - 13:59
        public static readonly (TimeSpan Start, TimeSpan End) Afternoon = (TimeSpan.FromHours(14), TimeSpan.FromHours(18)); // 14:00 - 17:59
        public static readonly (TimeSpan Start, TimeSpan End) Evening = (TimeSpan.FromHours(18), TimeSpan.FromHours(23));   // 18:00 - 22:59
        public static readonly (TimeSpan Start, TimeSpan End) Night = (TimeSpan.FromHours(23), TimeSpan.FromHours(24));
        public static int GetCurrentSession(TimeSpan currentTime)
        {
            if (currentTime >= Morning.Start && currentTime < Morning.End)
            {
                return 1; // Morning
            }
            if (currentTime >= Noon.Start && currentTime < Noon.End)
            {
                return 2; // Noon
            }
            if (currentTime >= Afternoon.Start && currentTime < Afternoon.End)
            {
                return 3; // Afternoon
            }
            if (currentTime >= Evening.Start && currentTime < Evening.End)
            {
                return 4; // Evening
            }
            if (currentTime >= Night.Start && currentTime < Night.End)
            {
                return 5; // Night
            }

            return -1; // Invalid session
        }
    }
}

