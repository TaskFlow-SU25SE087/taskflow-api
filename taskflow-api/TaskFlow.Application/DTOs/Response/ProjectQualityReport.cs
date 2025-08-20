namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class ProjectQualityReport
    {
        public Guid Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public DateTime AnalyzedAt { get; set; }
        public string AnalyzedBy { get; set; } = string.Empty;
        
        // Overall Project Quality
        public int OverallQualityScore { get; set; } // 0-100
        public string OverallGrade { get; set; } = string.Empty; // A, B, C, D, F
        public string OverallQualityLevel { get; set; } = string.Empty;
        
        // Project Statistics
        public int TotalFiles { get; set; }
        public int TotalLines { get; set; }
        public int TotalIssues { get; set; }
        public TimeSpan TotalAnalysisDuration { get; set; }
        
        // Quality Breakdown by Component
        public List<ComponentQualitySummary> ComponentBreakdown { get; set; } = new();
        
        // File-level Reports
        public List<CodeQualityReport> FileReports { get; set; } = new();
        
        // Top Issues
        public List<CommonIssue> TopIssues { get; set; } = new();
        
        // Quality Trends
        public string QualityTrend { get; set; } = string.Empty; // Improving, Stable, Declining
        public List<string> Recommendations { get; set; } = new();
    }
    
    public class ComponentQualitySummary
    {
        public string ComponentName { get; set; } = string.Empty; // Controllers, Services, Models, etc.
        public int QualityScore { get; set; }
        public string Grade { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public int TotalIssues { get; set; }
        public string Status { get; set; } = string.Empty; // Good, Needs Attention, Critical
    }
    
    public class CommonIssue
    {
        public string IssueType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int OccurrenceCount { get; set; }
        public string Severity { get; set; } = string.Empty;
        public string GlobalSuggestion { get; set; } = string.Empty;
    }
}
