using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Shared
{
    public class StatusAnimalEnum
    {
        public const string UnderTreatment = "UnderTreatment"; // Đang điều trị
        public const string LastDayMedication = "LastDayMedication"; // Ngày cuối uống thuốc
        public const string NotRecovered = "NotRecovered"; // Chưa khỏe
        public const string Recovered = "Recovered"; // Đã khỏe
    }
}
