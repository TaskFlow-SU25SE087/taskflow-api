using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class VerifyToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; } = null!;
        public Guid UserId { get; set; }
        public Guid? ProjectId { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public VerifyTokenEnum Type { get; set; }
        public bool IsUsed { get; set; } = false;
        public bool IsLocked { get; set; } = false;
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public int Attempts { get; set; } = 0;
    }
}
