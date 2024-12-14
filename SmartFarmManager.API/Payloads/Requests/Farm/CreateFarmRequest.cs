namespace SmartFarmManager.API.Payloads.Requests.Farm
{
    public class CreateFarmRequest
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public double Area { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class UpdateFarmRequest : CreateFarmRequest { }

}
