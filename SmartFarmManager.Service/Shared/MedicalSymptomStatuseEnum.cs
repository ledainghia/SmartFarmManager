using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Shared
{
    public class MedicalSymptomStatuseEnum
    {
        public const string Normal = "Normal"; //Bình thường
        public const string Diagnosed = "Diagnosed"; //Đã chuẩn đoán
        public const string Pending = "Pending"; //Đang chờ xem xét
        public const string Prescribed = "Prescribed"; //Đã kê thuốc

    }
}
