using taskflow_api.TaskFlow.Domain.Entities;

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
        
        // New methods for automatic distribution
        Task<List<TaskAssignee>> DistributeEffortPointsAutomatically(Guid taskId, int? taskEffortPoints);
        Task<List<TaskAssignee>> RedistributeEffortPointsAfterAssigneeLeaves(Guid taskId, int? taskEffortPoints);
        Task<int> CalculateTaskEffortPointsFromAssignees(Guid taskId);
        Task<bool> NeedsEffortPointsRedistribution(Guid taskId, int? taskEffortPoints);
        Task<List<TaskAssignee>> RedistributeEffortPointsOnTaskUpdate(Guid taskId, int? newTaskEffortPoints);
        Task<List<TaskAssignee>> UpdateIndividualAssigneeEffortPoints(Guid taskId, int taskEffortPoints, Guid targetProjectMemberId, int newAssignedEffortPoints);
    }
}

