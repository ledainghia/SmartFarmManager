using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Validation
{
    public class SessionValidatorAttribute:ValidationAttribute

    {
        private static readonly HashSet<string> ValidSessions = new HashSet<string>
        {
            "Morning",            
            "Afternoon",
            "Evening"
        };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string session && ValidSessions.Contains(session))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult($"The Session field must be one of the following values: {string.Join(", ", ValidSessions)}.");
        }
    }
}
