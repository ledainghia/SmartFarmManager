using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Shared
{
    public static class PrescriptionStatusEnum
    {
        public const string Active = "Active"; //Đơn thuốc đang được thực hiện
        public const string Completed = "Completed"; //Hoàn thành đơn thuốc
        public const string FollowUp = "FollowUp"; //Đã dùng xong, nhưng không khỏi, cần kê tiếp
        public const string Cancelled = "Cancelled";
        public const string Return = "Return"; //Đã chuyển về chuồng
        public const string Stop = "Stop"; //đừng khi đơn thuốc đang active
    }
}
