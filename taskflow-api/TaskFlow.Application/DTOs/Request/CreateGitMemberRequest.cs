using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Request
{
    public class CreateGitMemberRequest
    {
        public string NameLocal { get; set; } = string.Empty;
        public string EmailLocal { get; set; } = string.Empty;
    }
}
