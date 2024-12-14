using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username can't be longer than 50 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string Password { get; set; }
    }
}
