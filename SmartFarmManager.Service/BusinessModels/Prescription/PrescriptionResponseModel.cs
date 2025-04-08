using SmartFarmManager.Service.BusinessModels.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Prescription
{
    public class PrescriptionResponseModel
    {
        public Guid Id { get; set; }
        public Guid MedicalSymptomId { get; set; }
        public Guid CageId { get; set; }
        public DateTime? PrescribedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Notes { get; set; }
        public int QuantityAnimal { get; set; }
        public int? RemainingQuantity { get; set; }
        public string Status { get; set; }
        public int? DaysToTake { get; set; }
        public decimal? Price { get; set; }
        public string? cageAnimal { get; set; }

        public List<TaskResponseModel> Tasks { get; set; } = new List<TaskResponseModel>();
    }
}
