using SmartFarmManager.Service.BusinessModels.Vaccine;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.Vaccine
{
    public class CreateVaccineRequest
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Method is required.")]
        [StringLength(50, ErrorMessage = "Method cannot be longer than 50 characters.")]
        public string Method { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public double Price { get; set; }

        [Required(ErrorMessage = "AgeStart is required.")]
        public int AgeStart { get; set; }

        [Required(ErrorMessage = "AgeEnd is required.")]
        public int AgeEnd { get; set; }
        public CreateVaccineModel MapToModel()
        {
            return new CreateVaccineModel
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
