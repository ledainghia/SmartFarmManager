using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Helpers
{
    public static class DateTimeUtils
    {
        private static readonly TimeZoneInfo VietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

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
    }
}
