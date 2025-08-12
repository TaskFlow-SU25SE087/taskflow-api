using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class ProjectLogResponse
    {
        public Guid Id { get; set; }

        //User info
        public Guid ProjectMemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;


        public TypeLog ActionType { get; set; }
        public ChangedField? FieldChanged { get; set; }
        public string? OldValue { get; set; } = string.Empty;
        public string? NewValue { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;

        public DateTime CreateAt { get; set; }
    }
}
