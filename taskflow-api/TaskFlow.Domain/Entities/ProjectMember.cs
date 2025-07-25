﻿using System.ComponentModel.DataAnnotations;
using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class ProjectMember
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public ProjectRole Role { get; set; }
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public bool IsActive { get; set; } = false;
        public bool HasJoinedBefore { get; set; } = false;

        public List<TaskAssignee> taskUsers { get; set; } = new List<TaskAssignee>();
        public List<TaskComment> TaskComments { get; set; } = new();
        public List<GitMember> GitMembers { get; set; } = new List<GitMember>();
        public List<Issue> Issues { get; set; } = new List<Issue>();
    }
}
