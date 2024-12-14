namespace SmartFarmManager.API.Payloads.Requests.Cages
{
    public class CreateCageRequest
    {
        public Guid FarmId { get; set; }
        public string Name { get; set; }
        public double Area { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        public string AnimalType { get; set; }
    }

    public class UpdateCageRequest : CreateCageRequest { }

    

}
