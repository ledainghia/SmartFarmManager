using SmartFarmManager.Service.Shared;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Validation
{
    public class ValidSessionTypeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Session is required.");
            }

            if (!Enum.IsDefined(typeof(SessionTypeEnum), value))
            {
                return new ValidationResult($"Invalid session value. Allowed values are: {string.Join(", ", Enum.GetValues(typeof(SessionTypeEnum)).Cast<int>())}");
            }

            return ValidationResult.Success;
        }
    }
}
