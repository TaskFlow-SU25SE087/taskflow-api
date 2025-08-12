using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class TeamActivityReportRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Guid>? MemberIds { get; set; }
        public bool IncludeTaskDetails { get; set; } = true;
        public bool IncludeCommentDetails { get; set; } = true;
        public bool IncludeTopContributors { get; set; } = true;
        public int TopContributorsCount { get; set; } = 5;
    }
}
