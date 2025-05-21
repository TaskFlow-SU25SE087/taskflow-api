using System.ComponentModel.DataAnnotations;

namespace taskflow_api.Entity
{
    public class Issue
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TaskProjectID { get; set; }
        public TaskProject TaskProject { get; set; } = null!;
        [Required]
        public string title { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        public Issue()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
