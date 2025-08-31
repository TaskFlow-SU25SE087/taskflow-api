using taskflow_api.TaskFlow.Domain.Entities;

namespace taskflow_api.TaskFlow.Shared.Helpers
{
    public class CommitScoreCalculator
    {
        public static double CalculateQualityScore(CommitRecord commit)
        {
            var score =
                Math.Max(0.0, 5.0 - (commit.Bugs + commit.Vulnerabilities) * (5.0 / 30.0)) +
                Math.Max(0.0, 2.5 - commit.CodeSmells * (2.5 / 150.0)) +
                Math.Max(0.0, 1.0 - commit.DuplicatedLinesDensity / 100.0) +
                Math.Max(0.0, 1.5 - commit.SecurityHotspots * (1.5 / 20.0));
            return Math.Round(score, 2);
        }
    }
}
