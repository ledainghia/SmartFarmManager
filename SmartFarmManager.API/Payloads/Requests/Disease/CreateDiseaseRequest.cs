using SmartFarmManager.Service.BusinessModels.Disease;

namespace SmartFarmManager.API.Payloads.Requests.Disease
{
    public class CreateDiseaseRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public CreateDiseaseModel MapToModel()
        {
            return new CreateDiseaseModel
            {
                Name = this.Name,
                Description = this.Description
            };
        }
    }
}
