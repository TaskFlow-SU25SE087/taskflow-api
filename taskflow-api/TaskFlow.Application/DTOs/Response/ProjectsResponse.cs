using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class ProjectsResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public DateTime LastUpdate { get; set; }
        public ProjectRole? Role { get; set; }
    }
}
