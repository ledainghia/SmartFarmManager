namespace SmartFarmManager.API.Payloads.Responses.Farm
{
    public class FarmResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Area { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

}
