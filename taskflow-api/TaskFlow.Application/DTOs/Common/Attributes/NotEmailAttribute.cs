using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Common.Attributes
{
    public class NotEmailAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var username = value as string;
            if (username != null && username.Contains("@"))
            {
                return new ValidationResult("should not be the same as email");
            }
            return ValidationResult.Success!;
        }
    }
}
