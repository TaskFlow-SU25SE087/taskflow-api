using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class GitMemberResponse
    {
        public Guid Id { get; set; }

        public Guid? ProjectMemberId { get; set; }
        public string GitName { get; set; } = string.Empty;
        public string GitEmail { get; set; } = string.Empty;
        public string GitAvatarUrl { get; set; } = string.Empty;
        public string NameLocal { get; set; } = string.Empty;
        public string EmailLocal { get; set; } = string.Empty;
    }
}
