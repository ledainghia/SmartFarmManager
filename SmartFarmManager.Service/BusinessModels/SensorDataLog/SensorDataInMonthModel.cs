using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.SensorDataLog
{
    public class SensorDataInMonthModel
    {
        public int Year { get; set; }   // Năm
        public int Month { get; set; }  // Tháng
        public List<SensorDataInDayModel> Records { get; set; }  // Dữ liệu nhóm theo ngày trong tháng
    }
}
