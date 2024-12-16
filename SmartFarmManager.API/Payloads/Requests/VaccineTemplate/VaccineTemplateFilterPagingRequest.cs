using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.VaccineTemplate
{
    public class VaccineTemplateFilterPagingRequest
    {
        public Guid? TemplateId { get; set; } 
        public string? VaccineName { get; set; } 
        public int? Session { get; set; } 
        public int? ApplicationAge { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "PageNumber must be greater than 0.")]
        public int PageNumber { get; set; } = 1;

        [Required]
        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = 10;
    }
}
