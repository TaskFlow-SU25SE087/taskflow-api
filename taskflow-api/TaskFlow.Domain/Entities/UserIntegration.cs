using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class UserIntegration
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid UserId { get; set; }
        public string? GitHubAccessToken { get; set; }
    }
}
