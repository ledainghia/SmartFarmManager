namespace SmartFarmManager.API.Payloads.Requests.User
{
    public class VerifyOtpPhoneRequest
    {
        public string PhoneNumber { get; set; } = null!;
        public string Otp { get; set; } = null!;
    }
}
