using SmartFarmManager.Service.BusinessModels.Medication;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.Medication
{
    public class CreateMedicationRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Usage instructions are required.")]
        public string UsageInstructions { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a non-negative value.")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Dose quantity must be a non-negative value.")]
        public int? DoseQuantity { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price per dose must be a non-negative value.")]
        public decimal? PricePerDose { get; set; }

        public MedicationModel MapToModel()
        {
            return new MedicationModel
            {
                Name = Name,
                UsageInstructions = UsageInstructions,
                Price = Price,
                DoseQuantity = DoseQuantity,
                PricePerDose = PricePerDose
            };
        }
    }
    
}
