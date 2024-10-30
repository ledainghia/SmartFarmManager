namespace SmartFarmManager.API.Payloads.Responses.Auth
{
    public class LoginResponse
    {
        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;
    }
}
