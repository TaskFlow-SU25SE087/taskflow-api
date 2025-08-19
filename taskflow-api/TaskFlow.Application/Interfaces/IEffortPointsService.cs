namespace taskflow_api.TaskFlow.Application.Interfaces
{
    public interface IEffortPointsService
    {
        Task<bool> ValidateEffortPointsDistribution(Guid taskId, int? taskEffortPoints, int? newAssignedPoints);
        Task<bool> ValidateBulkEffortPointsDistribution(Guid taskId, int? taskEffortPoints, List<int?> assignedPointsList);
        Task<int> GetTotalAssignedEffortPoints(Guid taskId);
        Task<int> GetRemainingEffortPoints(Guid taskId, int? taskEffortPoints);
        List<int> DistributeEffortPointsEqually(int totalPoints, int numberOfAssignees);
        List<int> DistributeEffortPointsByPercentage(int totalPoints, List<double> percentages);
    }
}

