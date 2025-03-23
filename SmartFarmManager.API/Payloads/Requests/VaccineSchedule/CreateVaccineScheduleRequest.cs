using SmartFarmManager.Service.BusinessModels.VaccineSchedule;

namespace SmartFarmManager.API.Payloads.Requests.VaccineSchedule
{
    public class CreateVaccineScheduleRequest
    {
        public Guid VaccineId { get; set; }
        public Guid StageId { get; set; }
        public DateTime? Date { get; set; }
        public int? Quantity { get; set; }
        public int? ApplicationAge { get; set; }
        public int Session { get; set; }

        public CreateVaccineScheduleModel MapToModel()
        {
            return new CreateVaccineScheduleModel
            {
                VaccineId = this.VaccineId,
                StageId = this.StageId,
                Date = this.Date,
                Quantity = this.Quantity,
                ApplicationAge = this.ApplicationAge,
                Session = this.Session,
            };
        }
    }

}
