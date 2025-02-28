namespace SmartFarmManager.API.Payloads.Requests.User
{
    public class SendOptPhoneRequest
    {
        public string UserName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public bool IsResend { get; set; } // True if it's a resend request
    }
}
