namespace SmartFarmManager.API.Payloads.Responses.Auth
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string Address { get; set; }

    }

    public class UserProfileResponse : UserResponse
    {
        public DateTime CreatedAt { get; set; }
    }
}
