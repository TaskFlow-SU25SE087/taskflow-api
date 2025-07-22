using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class UpdateTerm
    {
        [Required(ErrorMessage = "Please enter the term name")]
        public string Season { get; set; } = null!;
        [Required(ErrorMessage = "Please enter the term year")]
        public int Year { get; set; }
        [Required(ErrorMessage = "Please enter the start date of the term")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "Please enter the end date of the term")]
        public DateTime EndDate { get; set; }
    }
}
