using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Prescription
{
    public class PrescriptionDetail
    {
        public Guid PrescriptionId { get; set; }
        public string Diagnosis { get; set; }
        public int AffectedQuantity { get; set; }
        public decimal PrescriptionPrice { get; set; }
        public string DiseaseName { get; set; } // ✅ Thêm tên bệnh (Disease)
        public string DiseaseDescription { get; set; } // ✅ Thêm mô tả bệnh
        public List<string> Symptoms { get; set; }
    }
}
