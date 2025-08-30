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

        public BoardIdLogResponse? Board { get; set; } = null;
        public SprintIdLogResponse? Sprint { get; set; } = null;
        public TaskIdLogResponse? Task { get; set; } = null;

        public string? OldValue { get; set; } = string.Empty;
        public string? NewValue { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;

        public DateTime CreateAt { get; set; }
    }

    public class BoardIdLogResponse
    {
        public Guid? BoardId { get; set; } = null;
        public string? BoardName { get; set; } = string.Empty;
    }

    public class SprintIdLogResponse
    {
        public Guid? SprintId { get; set; } = null;
        public string? SprintName { get; set; } = string.Empty;
    }

    public class TaskIdLogResponse
    {
        public Guid? TaskId { get; set; } = null;
        public string? TaskName { get; set; } = string.Empty;
    }
}
