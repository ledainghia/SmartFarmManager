using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Prescription
{
    public class UpdatePrescriptionModel
    {
        public string Status { get; set; } // Completed | Dead
        public int? RemainingQuantity { get; set; } // Số lượng vật nuôi phục hồi (chỉ dùng khi status = Completed)
    }

}
