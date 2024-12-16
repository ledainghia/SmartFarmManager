namespace SmartFarmManager.API.Payloads.Requests.Auth
{
    public class CreateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public Guid RoleId { get; set; }
    }
}
