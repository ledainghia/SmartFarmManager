using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.Vaccine
{
    public class VaccineFilterRequest
    {
        [StringLength(100, ErrorMessage = "KeySearch cannot be longer than 100 characters.")]
        public string? KeySearch { get; set; }
        public int? AgeStart { get; set; }
        public int? AgeEnd { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
