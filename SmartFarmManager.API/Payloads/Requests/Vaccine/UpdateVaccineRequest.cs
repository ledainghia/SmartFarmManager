using SmartFarmManager.Service.BusinessModels.Vaccine;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.Vaccine
{
    public class UpdateVaccineRequest
    {
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string? Name { get; set; }
        [StringLength(50, ErrorMessage = "Method cannot be longer than 50 characters.")]
        public string? Method { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public double? Price { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }
        public VaccineUpdateModel MapToModel()
        {
            return new VaccineUpdateModel
            {
                Name = Name,
                Method = Method,
                Price = Price,
                AgeStart = AgeStart,
                AgeEnd = AgeEnd
            };
        }
    }
}
