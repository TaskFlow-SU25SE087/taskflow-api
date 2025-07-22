using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class OneProjectResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdate { get; set; }
        public List<Board> Boards { get; set; } = new List<Board>();
        public List<Sprint> Sprints { get; set; } = new List<Sprint>();
        public List<TaskProject> TaskProject { get; set; } = new List<TaskProject>();
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}
