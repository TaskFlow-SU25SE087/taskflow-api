using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using taskflow_api.TaskFlow.Application.DTOs.Response;
using taskflow_api.TaskFlow.Application.Interfaces;
using System.IO.Compression;
using System.Text;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class CodeQualityService : ICodeQualityService
    {
        private readonly ILogger<CodeQualityService> _logger;

        public CodeQualityService(ILogger<CodeQualityService> logger)
        {
            _logger = logger;
        }

        public async Task<CodeQualityReport> AnalyzeSingleFileAsync(IFormFile file, string userId)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty or null");

                if (!file.FileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Only C# files are supported");

                var sourceCode = await ReadFileContentAsync(file);
                var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
                var root = await syntaxTree.GetRootAsync();

                var report = new CodeQualityReport
                {
                    Id = Guid.NewGuid(),
                    FileName = file.FileName,
                    ProjectName = Path.GetFileNameWithoutExtension(file.FileName),
                    AnalyzedAt = DateTime.UtcNow,
                    AnalyzedBy = userId,
                    AnalysisType = "single"
                };

                // Analyze basic metrics
                AnalyzeBasicMetrics(root, report);
                
                // Analyze complexity
                AnalyzeComplexity(root, report);
                
                // Analyze code quality issues
                AnalyzeCodeQualityIssues(root, report);
                
                // Calculate quality score
                CalculateQualityScore(report);
                
                // Generate suggestions
                GenerateSuggestions(report);
                
                report.AnalysisDuration = DateTime.UtcNow - startTime;
                
                _logger.LogInformation("Analyzed file {FileName} for user {UserId} in {Duration}ms", 
                    file.FileName, userId, report.AnalysisDuration.TotalMilliseconds);
                
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing single file {FileName}", file?.FileName);
                throw;
            }
        }

        public async Task<ProjectQualityReport> AnalyzeProjectAsync(IFormFile projectZip, string userId, string projectName)
        {
            var startTime = DateTime.UtcNow;
            
            try
            {
                if (projectZip == null || projectZip.Length == 0)
                    throw new ArgumentException("Project file is empty or null");

                var projectReport = new ProjectQualityReport
                {
                    Id = Guid.NewGuid(),
                    ProjectName = projectName,
                    AnalyzedAt = DateTime.UtcNow,
                    AnalyzedBy = userId
                };

                var fileReports = new List<CodeQualityReport>();
                var componentBreakdown = new Dictionary<string, ComponentQualitySummary>();

                using var zipArchive = new ZipArchive(projectZip.OpenReadStream(), ZipArchiveMode.Read);
                
                foreach (var entry in zipArchive.Entries)
                {
                    if (entry.Name.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var sourceCode = await ReadZipEntryContentAsync(entry);
                            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
                            var root = await syntaxTree.GetRootAsync();

                            var fileReport = new CodeQualityReport
                            {
                                Id = Guid.NewGuid(),
                                FileName = entry.FullName,
                                ProjectName = projectName,
                                AnalyzedAt = DateTime.UtcNow,
                                AnalyzedBy = userId,
                                AnalysisType = "project"
                            };

                            AnalyzeBasicMetrics(root, fileReport);
                            AnalyzeComplexity(root, fileReport);
                            AnalyzeCodeQualityIssues(root, fileReport);
                            CalculateQualityScore(fileReport);
                            GenerateSuggestions(fileReport);

                            fileReports.Add(fileReport);
                            
                            // Group by component type
                            var componentName = GetComponentName(entry.FullName);
                            if (!componentBreakdown.ContainsKey(componentName))
                            {
                                componentBreakdown[componentName] = new ComponentQualitySummary
                                {
                                    ComponentName = componentName,
                                    FileCount = 0,
                                    TotalIssues = 0,
                                    QualityScore = 0
                                };
                            }
                            
                            componentBreakdown[componentName].FileCount++;
                            componentBreakdown[componentName].TotalIssues += fileReport.TotalIssues;
                            componentBreakdown[componentName].QualityScore += fileReport.QualityScore;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to analyze file {FileName} in project", entry.FullName);
                        }
                    }
                }

                // Calculate component averages and overall project score
                foreach (var component in componentBreakdown.Values)
                {
                    component.QualityScore = component.FileCount > 0 ? component.QualityScore / component.FileCount : 0;
                    component.Grade = CalculateGrade(component.QualityScore);
                    component.Status = GetComponentStatus(component.QualityScore);
                }

                projectReport.FileReports = fileReports;
                projectReport.ComponentBreakdown = componentBreakdown.Values.ToList();
                projectReport.TotalFiles = fileReports.Count;
                projectReport.TotalLines = fileReports.Sum(f => f.TotalLines);
                projectReport.TotalIssues = fileReports.Sum(f => f.TotalIssues);
                projectReport.OverallQualityScore = fileReports.Count > 0 ? (int)fileReports.Average(f => f.QualityScore) : 0;
                projectReport.OverallGrade = CalculateGrade(projectReport.OverallQualityScore);
                projectReport.OverallQualityLevel = GetQualityLevel(projectReport.OverallQualityScore);
                projectReport.TotalAnalysisDuration = DateTime.UtcNow - startTime;

                // Generate top issues and recommendations
                GenerateProjectRecommendations(projectReport);

                _logger.LogInformation("Analyzed project {ProjectName} for user {UserId} in {Duration}ms", 
                    projectName, userId, projectReport.TotalAnalysisDuration.TotalMilliseconds);

                return projectReport;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing project {ProjectName}", projectName);
                throw;
            }
        }

        public async Task<ProjectQualityReport> AnalyzeGitHubRepoAsync(string repoUrl, string userId)
        {
            // This would integrate with GitHub API to clone and analyze repos
            // For now, return a placeholder implementation
            throw new NotImplementedException("GitHub repository analysis will be implemented in the next phase");
        }

        public async Task<List<CodeQualityReport>> GetUserQualityHistoryAsync(string userId)
        {
            // This would query the database for user's quality history
            // For now, return empty list
            return new List<CodeQualityReport>();
        }

        public async Task<List<ProjectQualityReport>> GetProjectQualityHistoryAsync(string projectName)
        {
            // This would query the database for project's quality history
            // For now, return empty list
            return new List<ProjectQualityReport>();
        }

        public async Task<CodeQualityReport?> GetQualityReportAsync(Guid reportId)
        {
            // This would query the database for a specific report
            // For now, return null
            return null;
        }

        public async Task<ProjectQualityReport?> GetProjectQualityReportAsync(Guid reportId)
        {
            // This would query the database for a specific project report
            // For now, return null
            return null;
        }

        #region Private Methods

        private async Task<string> ReadFileContentAsync(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            return await reader.ReadToEndAsync();
        }

        private async Task<string> ReadZipEntryContentAsync(ZipArchiveEntry entry)
        {
            using var stream = entry.Open();
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        private void AnalyzeBasicMetrics(SyntaxNode root, CodeQualityReport report)
        {
            var lines = root.ToString().Split('\n');
            report.TotalLines = lines.Length;
            report.CodeLines = lines.Count(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("//") && !line.TrimStart().StartsWith("/*"));
            report.CommentLines = lines.Count(line => line.TrimStart().StartsWith("//") || line.TrimStart().StartsWith("/*"));
            report.BlankLines = lines.Count(line => string.IsNullOrWhiteSpace(line));
        }

        private void AnalyzeComplexity(SyntaxNode root, CodeQualityReport report)
        {
            var visitor = new ComplexityVisitor();
            visitor.Visit(root);
            
            report.CyclomaticComplexity = visitor.CyclomaticComplexity;
            report.CognitiveComplexity = visitor.CognitiveComplexity;
            report.MaxNestingDepth = visitor.MaxNestingDepth;
        }

        private void AnalyzeCodeQualityIssues(SyntaxNode root, CodeQualityReport report)
        {
            var issues = new List<CodeQualityIssue>();
            
            // Check for long methods
            var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();
            foreach (var method in methods)
            {
                var methodLines = method.ToString().Split('\n').Length;
                if (methodLines > 30)
                {
                    issues.Add(new CodeQualityIssue
                    {
                        Type = "Warning",
                        Severity = "Medium",
                        Message = $"Method '{method.Identifier.Text}' is too long ({methodLines} lines)",
                        LineNumber = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                        Suggestion = "Consider breaking this method into smaller, more focused methods",
                        CodeExample = "// Break into smaller methods:\n// - ValidateInput()\n// - ProcessData()\n// - SaveResult()"
                    });
                }
            }

            // Check for deep nesting
            if (report.MaxNestingDepth > 4)
            {
                issues.Add(new CodeQualityIssue
                {
                    Type = "Warning",
                    Severity = "Medium",
                    Message = $"Code has deep nesting ({report.MaxNestingDepth} levels)",
                    LineNumber = 1,
                    Suggestion = "Consider using early returns or extracting methods to reduce nesting",
                    CodeExample = "// Instead of:\nif (condition1) {\n  if (condition2) {\n    if (condition3) {\n      // code\n    }\n  }\n}\n\n// Use:\nif (!condition1) return;\nif (!condition2) return;\nif (!condition3) return;\n// code"
                });
            }

            // Check for magic numbers
            var literals = root.DescendantNodes().OfType<LiteralExpressionSyntax>();
            foreach (var literal in literals.Where(l => l.Token.ValueText.All(char.IsDigit) && int.Parse(l.Token.ValueText) > 10))
            {
                issues.Add(new CodeQualityIssue
                {
                    Type = "Info",
                    Severity = "Low",
                    Message = $"Magic number detected: {literal.Token.ValueText}",
                    LineNumber = literal.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
                    Suggestion = "Consider defining this as a named constant",
                    CodeExample = $"private const int {GetConstantName(literal.Token.ValueText)} = {literal.Token.ValueText};"
                });
            }

            report.Issues = issues;
            report.TotalIssues = issues.Count;
            report.CriticalIssues = issues.Count(i => i.Severity == "Critical");
            report.WarningIssues = issues.Count(i => i.Severity == "High" || i.Severity == "Medium");
            report.InfoIssues = issues.Count(i => i.Severity == "Low");
        }

        private void CalculateQualityScore(CodeQualityReport report)
        {
            var score = 100;
            
            // Penalty for complexity
            if (report.CyclomaticComplexity > 10) score -= 20;
            else if (report.CyclomaticComplexity > 5) score -= 10;
            
            // Penalty for long methods
            if (report.CodeLines > 100) score -= 15;
            else if (report.CodeLines > 50) score -= 10;
            
            // Penalty for deep nesting
            if (report.MaxNestingDepth > 4) score -= 15;
            else if (report.MaxNestingDepth > 3) score -= 10;
            
            // Penalty for issues
            score -= report.CriticalIssues * 10;
            score -= report.WarningIssues * 5;
            score -= report.InfoIssues * 2;
            
            report.QualityScore = Math.Max(0, score);
            report.Grade = CalculateGrade(report.QualityScore);
            report.QualityLevel = GetQualityLevel(report.QualityScore);
        }

        private string CalculateGrade(int score)
        {
            return score switch
            {
                >= 90 => "A",
                >= 80 => "B",
                >= 70 => "C",
                >= 60 => "D",
                _ => "F"
            };
        }

        private string GetQualityLevel(int score)
        {
            return score switch
            {
                >= 90 => "Excellent",
                >= 80 => "Good",
                >= 70 => "Fair",
                >= 60 => "Poor",
                _ => "Critical"
            };
        }

        private string GetComponentStatus(int score)
        {
            return score switch
            {
                >= 80 => "Good",
                >= 60 => "Needs Attention",
                _ => "Critical"
            };
        }

        private string GetComponentName(string filePath)
        {
            if (filePath.Contains("Controller")) return "Controllers";
            if (filePath.Contains("Service")) return "Services";
            if (filePath.Contains("Repository")) return "Repositories";
            if (filePath.Contains("Model") || filePath.Contains("Entity")) return "Models";
            if (filePath.Contains("DTO")) return "DTOs";
            if (filePath.Contains("Interface") || filePath.Contains("I")) return "Interfaces";
            return "Other";
        }

        private string GetConstantName(string value)
        {
            return $"MAX_{value.ToUpper()}";
        }

        private void GenerateSuggestions(CodeQualityReport report)
        {
            var suggestions = new List<string>();
            
            if (report.CyclomaticComplexity > 5)
                suggestions.Add("Consider simplifying complex methods by extracting smaller, focused methods");
            
            if (report.MaxNestingDepth > 3)
                suggestions.Add("Reduce nesting by using early returns or extracting methods");
            
            if (report.CodeLines > 50)
                suggestions.Add("Break large methods into smaller, more manageable pieces");
            
            if (report.CriticalIssues > 0)
                suggestions.Add("Address critical issues first as they may cause runtime problems");
            
            if (report.WarningIssues > 2)
                suggestions.Add("Fix warning issues to improve code maintainability");
            
            if (suggestions.Count == 0)
                suggestions.Add("Great job! Your code follows good practices. Keep it up!");
            
            report.Suggestions = suggestions;
        }

        private void GenerateProjectRecommendations(ProjectQualityReport report)
        {
            var recommendations = new List<string>();
            var topIssues = new List<CommonIssue>();
            
            // Group issues by type
            var issueGroups = report.FileReports
                .SelectMany(f => f.Issues)
                .GroupBy(i => i.Message)
                .OrderByDescending(g => g.Count())
                .Take(5);
            
            foreach (var group in issueGroups)
            {
                topIssues.Add(new CommonIssue
                {
                    IssueType = group.First().Type,
                    Message = group.Key,
                    OccurrenceCount = group.Count(),
                    Severity = group.First().Severity,
                    GlobalSuggestion = group.First().Suggestion
                });
            }
            
            report.TopIssues = topIssues;
            
            // Generate recommendations based on overall quality
            if (report.OverallQualityScore < 60)
                recommendations.Add("Focus on improving code quality fundamentals before adding new features");
            else if (report.OverallQualityScore < 80)
                recommendations.Add("Good progress! Focus on reducing complexity and improving readability");
            else
                recommendations.Add("Excellent code quality! Consider adding unit tests and documentation");
            
            report.Recommendations = recommendations;
        }

        #endregion
    }

    #region Helper Classes

    public class ComplexityVisitor : CSharpSyntaxWalker
    {
        public int CyclomaticComplexity { get; private set; } = 1; // Base complexity
        public int CognitiveComplexity { get; private set; } = 0;
        public int MaxNestingDepth { get; private set; } = 0;
        private int _currentNestingDepth = 0;

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            CyclomaticComplexity++;
            CognitiveComplexity++;
            _currentNestingDepth++;
            MaxNestingDepth = Math.Max(MaxNestingDepth, _currentNestingDepth);
            base.VisitIfStatement(node);
            _currentNestingDepth--;
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            CyclomaticComplexity++;
            CognitiveComplexity++;
            _currentNestingDepth++;
            MaxNestingDepth = Math.Max(MaxNestingDepth, _currentNestingDepth);
            base.VisitForStatement(node);
            _currentNestingDepth--;
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            CyclomaticComplexity++;
            CognitiveComplexity++;
            _currentNestingDepth++;
            MaxNestingDepth = Math.Max(MaxNestingDepth, _currentNestingDepth);
            base.VisitForEachStatement(node);
            _currentNestingDepth--;
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            CyclomaticComplexity++;
            CognitiveComplexity++;
            _currentNestingDepth++;
            MaxNestingDepth = Math.Max(MaxNestingDepth, _currentNestingDepth);
            base.VisitWhileStatement(node);
            _currentNestingDepth--;
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            CyclomaticComplexity++;
            CognitiveComplexity++;
            _currentNestingDepth++;
            MaxNestingDepth = Math.Max(MaxNestingDepth, _currentNestingDepth);
            base.VisitDoStatement(node);
            _currentNestingDepth--;
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            CyclomaticComplexity += node.Sections.Count;
            CognitiveComplexity += node.Sections.Count;
            _currentNestingDepth++;
            MaxNestingDepth = Math.Max(MaxNestingDepth, _currentNestingDepth);
            base.VisitSwitchStatement(node);
            _currentNestingDepth--;
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            CyclomaticComplexity++;
            CognitiveComplexity++;
            base.VisitConditionalExpression(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.OperatorToken.ValueText == "&&" || node.OperatorToken.ValueText == "||")
            {
                CognitiveComplexity++;
            }
            base.VisitBinaryExpression(node);
        }
    }

    #endregion
}
