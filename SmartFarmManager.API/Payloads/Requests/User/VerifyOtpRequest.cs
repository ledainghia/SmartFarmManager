namespace SmartFarmManager.API.Payloads.Requests.User
{
    public class VerifyOtpRequest
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }
}
