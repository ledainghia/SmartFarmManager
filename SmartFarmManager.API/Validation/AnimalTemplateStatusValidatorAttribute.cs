using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Validation
{
    public class AnimalTemplateStatusValidatorAttribute: ValidationAttribute
    {
        private readonly string[] _validStatuses = { "Draft", "Active", "Inactive" };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string status)
            {
                if (Array.Exists(_validStatuses, s => s.Equals(status, StringComparison.OrdinalIgnoreCase)))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult($"Invalid status. Allowed values are: {string.Join(", ", _validStatuses)}");
        }
    }
}
