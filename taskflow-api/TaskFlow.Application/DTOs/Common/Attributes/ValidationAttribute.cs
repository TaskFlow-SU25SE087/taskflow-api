using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Common.Attributes
{
    public class MaxFileCountAttribute : ValidationAttribute
    {
        private readonly int _maxCount;

        public MaxFileCountAttribute(int maxCount)
        {
            _maxCount = maxCount;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var files = value as List<IFormFile>;
            if (files != null && files.Count > _maxCount)
            {
                return new ValidationResult($"Maximum upload only {_maxCount} file.");
            }

            return ValidationResult.Success;
        }
    }

}
