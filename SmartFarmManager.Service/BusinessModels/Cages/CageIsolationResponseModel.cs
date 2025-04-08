using SmartFarmManager.Service.BusinessModels.Prescription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Cages
{
    public class CageIsolationResponseModel
    {
        public Guid Id { get; set; }
        public string PenCode { get; set; } // Mã chuồng để lấy data
        public Guid FarmId { get; set; }
        public string Name { get; set; }
        public double Area { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public string BoardCode { get; set; } // ??? 
        public bool BoardStatus { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string CameraUrl { get; set; }
        public int ChannelId { get; set; } // ??? 
        public bool IsSolationCage { get; set; } = false;

        // Associated data
        public List<PrescriptionResponseModel> Prescriptions { get; set; } = new List<PrescriptionResponseModel>();
    }
}
