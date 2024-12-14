namespace SmartFarmManager.API.Payloads.Responses.Cage
{
    public class CageResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Area { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        public string AnimalType { get; set; }
    }
}
