using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class TagResporn
    {
        public Guid Id { get; set; } = new Guid();
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
