﻿using System.ComponentModel.DataAnnotations;

namespace taskflow_api.TaskFlow.Domain.Entities
{
    public class CommitScanIssue
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CommitRecordId { get; set; }
        public CommitRecord CommitRecord { get; set; } = null!;

        [Required]
        public string Rule { get; set; } = string.Empty;
        [Required]
        public string Severity { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;
        public int? Line { get; set; }
        public string? LineContent { get; set; } = string.Empty;

        public string? BlamedGitEmail { get; set; } = string.Empty;
        public string? BlamedGitName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
