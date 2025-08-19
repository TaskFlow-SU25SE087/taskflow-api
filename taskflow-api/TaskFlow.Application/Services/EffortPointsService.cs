using taskflow_api.TaskFlow.Application.Interfaces;
using taskflow_api.TaskFlow.Domain.Common.Enums;
using taskflow_api.TaskFlow.Domain.Entities;
using taskflow_api.TaskFlow.Infrastructure.Interfaces;
using taskflow_api.TaskFlow.Shared.Exceptions;

namespace taskflow_api.TaskFlow.Application.Services
{
    public class EffortPointsService : IEffortPointsService
    {
        private readonly ITaskAssigneeRepository _taskAssigneeRepository;

        public EffortPointsService(ITaskAssigneeRepository taskAssigneeRepository)
        {
            _taskAssigneeRepository = taskAssigneeRepository;
        }

        public async Task<bool> ValidateEffortPointsDistribution(Guid taskId, int? taskEffortPoints, int? newAssignedPoints)
        {
            if (!taskEffortPoints.HasValue)
            {
                return true; // No effort points on task, so no validation needed
            }

            // Get current total assigned effort points
            var currentAssignees = await _taskAssigneeRepository.taskAssigneesAsync(taskId);
            var currentTotalAssigned = currentAssignees.Sum(a => a.AssignedEffortPoints ?? 0);
            var newTotal = currentTotalAssigned + (newAssignedPoints ?? 0);
            
            return newTotal <= taskEffortPoints.Value;
        }

        public async Task<bool> ValidateBulkEffortPointsDistribution(Guid taskId, int? taskEffortPoints, List<int?> assignedPointsList)
        {
            if (!taskEffortPoints.HasValue)
            {
                return true; // No effort points on task, so no validation needed
            }

            var totalAssignedPoints = assignedPointsList.Sum(p => p ?? 0);
            return totalAssignedPoints == taskEffortPoints.Value;
        }

        public async Task<int> GetTotalAssignedEffortPoints(Guid taskId)
        {
            var assignees = await _taskAssigneeRepository.taskAssigneesAsync(taskId);
            return assignees.Sum(a => a.AssignedEffortPoints ?? 0);
        }

        public async Task<int> GetRemainingEffortPoints(Guid taskId, int? taskEffortPoints)
        {
            if (!taskEffortPoints.HasValue)
            {
                return 0;
            }

            var totalAssigned = await GetTotalAssignedEffortPoints(taskId);
            return taskEffortPoints.Value - totalAssigned;
        }

        public List<int> DistributeEffortPointsEqually(int totalPoints, int numberOfAssignees)
        {
            if (numberOfAssignees <= 0)
            {
                throw new AppException(ErrorCode.InvalidEffortPointsDistribution);
            }

            var basePoints = totalPoints / numberOfAssignees;
            var remainder = totalPoints % numberOfAssignees;
            
            var distribution = new List<int>();
            for (int i = 0; i < numberOfAssignees; i++)
            {
                distribution.Add(basePoints + (i < remainder ? 1 : 0));
            }
            
            return distribution;
        }

        public List<int> DistributeEffortPointsByPercentage(int totalPoints, List<double> percentages)
        {
            if (percentages.Sum() != 100.0)
            {
                throw new AppException(ErrorCode.InvalidEffortPointsDistribution);
            }

            var distribution = new List<int>();
            var remainingPoints = totalPoints;
            
            for (int i = 0; i < percentages.Count; i++)
            {
                var points = (int)Math.Round(totalPoints * percentages[i] / 100.0);
                if (i == percentages.Count - 1)
                {
                    points = remainingPoints; // Give remaining points to last assignee
                }
                distribution.Add(points);
                remainingPoints -= points;
            }
            
            return distribution;
        }
    }
}
