    using System.ComponentModel.DataAnnotations;
    using taskflow_api.TaskFlow.Domain.Common.Enums;

    namespace taskflow_api.TaskFlow.Domain.Entities
    {
        public class CommitRecord
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();
            public Guid ProjectPartId { get; set; }
            public ProjectPart ProjectPart { get; set; } = null!;

            [Required]
            public string CommitId { get; set; } = string.Empty;
            [Required]

            public string Pusher { get; set; } = string.Empty;
            public DateTime PushedAt { get; set; } = DateTime.UtcNow;
            public StatusCommit Status { get; set; } = StatusCommit.Checking;

            public string? CommitUrl { get; set; }
            public string? CommitMessage { get; set; }
            public string? ResultSummary { get; set; }
            public DateTime? ExpectedFinishAt { get; set; }

            public string QualityGateStatus { get; set; } = string.Empty;
            public int Bugs { get; set; } = 0;
            public int Vulnerabilities { get; set; } = 0;
            public int CodeSmells { get; set; } = 0;
            public int SecurityHotspots { get; set; } = 0;
            public int DuplicatedLines { get; set; } = 0;
            public int DuplicatedBlocks { get; set; } = 0;
            public double DuplicatedLinesDensity { get; set; } = 0.0;
            public double Coverage { get; set; } = 0.0;
            public TimeSpan ScanDuration { get; set; } = TimeSpan.Zero;

        public bool Result { get; set; } = false;
            public string OutputLog { get; set; } = string.Empty;
            public string ErrorLog { get; set; } = string.Empty;
            public string ProjectKey { get; set; } = string.Empty;

            public List<CommitScanIssue> CommitScanIssues { get; set; } = new List<CommitScanIssue>();

        }
    }
