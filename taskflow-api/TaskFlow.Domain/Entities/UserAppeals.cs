using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class UserAppeals
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime AppealDate { get; set; } = DateTime.UtcNow;
        public bool IsSuccessful { get; set; } = false;
    }
}
