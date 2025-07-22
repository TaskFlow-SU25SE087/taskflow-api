using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class MemberResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } =null!;
        public string Avatar { get; set; } = null!;
        public string Email { get; set; } = null!;
        public ProjectRole Role { get; set; } = ProjectRole.Member;

    }
}
