namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class CodeQualityReport
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public DateTime AnalyzedAt { get; set; }
        public string AnalyzedBy { get; set; } = string.Empty;
        
        // Quality Score
        public int QualityScore { get; set; } // 0-100
        public string Grade { get; set; } = string.Empty; // A, B, C, D, F
        public string QualityLevel { get; set; } = string.Empty; // Excellent, Good, Fair, Poor, Critical
        
        // Basic Metrics
        public int TotalLines { get; set; }
        public int CodeLines { get; set; }
        public int CommentLines { get; set; }
        public int BlankLines { get; set; }
        
        // Complexity Metrics
        public int CyclomaticComplexity { get; set; }
        public int CognitiveComplexity { get; set; }
        public int MaxNestingDepth { get; set; }
        
        // Code Quality Issues
        public List<CodeQualityIssue> Issues { get; set; } = new();
        public int TotalIssues { get; set; }
        public int CriticalIssues { get; set; }
        public int WarningIssues { get; set; }
        public int InfoIssues { get; set; }
        
        // Suggestions
        public List<string> Suggestions { get; set; } = new();
        
        // Analysis Details
        public string AnalysisType { get; set; } = string.Empty; // single, project, github
        public TimeSpan AnalysisDuration { get; set; }
    }
    
    public class CodeQualityIssue
    {
        public string Type { get; set; } = string.Empty; // Error, Warning, Info
        public string Severity { get; set; } = string.Empty; // Critical, High, Medium, Low
        public string Message { get; set; } = string.Empty;
        public int LineNumber { get; set; }
        public string Suggestion { get; set; } = string.Empty;
        public string CodeExample { get; set; } = string.Empty;
    }
}
