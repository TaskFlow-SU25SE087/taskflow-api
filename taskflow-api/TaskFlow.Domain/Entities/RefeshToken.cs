using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class RefeshToken
    {
        [Key]
        public Guid Id { get; set; }
        [ForeignKey(nameof(UserId))]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Token { get; set; } = string.Empty;
        public string JwtID { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime IssueAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
