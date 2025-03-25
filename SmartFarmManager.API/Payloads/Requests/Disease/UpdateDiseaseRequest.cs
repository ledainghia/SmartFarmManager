using SmartFarmManager.Service.BusinessModels.Disease;

namespace SmartFarmManager.API.Payloads.Requests.Disease
{
    public class UpdateDiseaseRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }

        public UpdateDiseaseModel MapToModel()
        {
            return new UpdateDiseaseModel
            {
                Name = this.Name,
                Description = this.Description
            };
        }
    }
}
