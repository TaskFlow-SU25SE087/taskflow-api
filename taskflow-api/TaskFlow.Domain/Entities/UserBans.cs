using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class UserBans
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public DateTime BanDate { get; set; }
        public DateTime BanEndDate { get; set; }
        public bool IsPermanent { get; set; } = false;

        public List<UserAppeals> Appeals { get; set; } = new();
    }
}
