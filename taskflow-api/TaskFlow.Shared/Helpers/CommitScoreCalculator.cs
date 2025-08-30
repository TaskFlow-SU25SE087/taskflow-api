using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Shared.Helpers
{
    public class CommitScoreCalculator
    {
        public static double CalculateQualityScore(CommitRecord commit)
        {
            var score =
                Math.Min((commit.Coverage / 100.0) * 4.0, 4.0) +
                Math.Max(0.0, 3.0 - (commit.Bugs + commit.Vulnerabilities) * 0.1) +
                Math.Max(0.0, 1.5 - commit.CodeSmells * 0.01) +
                Math.Max(0.0, 0.5 - commit.DuplicatedLinesDensity / 100.0) +
                Math.Max(0.0, 1.0 - commit.SecurityHotspots * 0.05);
            return Math.Round(score, 2);
        }
    }
}
