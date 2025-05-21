using System.ComponentModel.DataAnnotations;

namespace taskflow_api.Entity
{
    public class LogProject


    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public Guid ProjectMemberId { get; set; }
        [Required]
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty;

        //changes Board
        public Guid? OldBoard { get; set; }
        public Guid? NewBoard { get; set; }

        //Assignee
        public Guid? Assigner { get; set; }
        public Guid? TaskProjectID { get; set; }
        public TaskProject TaskProject { get; set; } = null!;

    }
}
