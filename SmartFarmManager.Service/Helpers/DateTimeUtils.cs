using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Helpers
{
    public static class DateTimeUtils
    {

        private static TimeSpan _timeDifference = TimeSpan.Zero;
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        public static void SetTimeDifference(TimeSpan timeDifference)
        {
            _timeDifference = timeDifference;
        }
        /// <summary>
        /// Chuyển đổi DateTime.UtcNow sang giờ Việt Nam
        /// </summary>
        /// <returns>Thời gian hiện tại ở Việt Nam</returns>
        public static DateTime ConvertToVietnamTime(DateTime utcDateTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, VietnamTimeZone);
        }

        /// <summary>
        /// Lấy thời gian hiện tại ở Việt Nam
        /// </summary>
        /// <returns>Thời gian hiện tại ở Việt Nam</returns>
        public static DateTime VietnamNow()
        {
            return ConvertToVietnamTime(DateTime.UtcNow);
        }
        public static DateTime GetServerTimeInVietnamTime()
        {
            // Lấy giờ theo múi giờ server
            DateTime serverTime = DateTime.Now;

            // Chuyển giờ server sang múi giờ UTC
            DateTimeOffset serverTimeOffset = new DateTimeOffset(serverTime, TimeZoneInfo.Local.GetUtcOffset(serverTime));

            // Chuyển từ giờ server sang múi giờ Việt Nam
            DateTime vietnamTime=TimeZoneInfo.ConvertTime(serverTimeOffset, VietnamTimeZone).DateTime;

            vietnamTime = vietnamTime.Add(_timeDifference);
            return vietnamTime;
        }
    }
}
