using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Shared
{
    public class MedicalSymptomStatuseEnum
    {
        public const string Rejected = "Rejected"; //Từ chối - Bình thường
        public const string Pending = "Pending"; //Đang chờ xem xét
        public const string Prescribed = "Prescribed"; //Đã kê thuốc

    }
}
