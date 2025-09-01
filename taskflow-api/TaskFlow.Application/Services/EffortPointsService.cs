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

        /// <summary>
        /// Automatically distributes effort points equally among all assignees of a task
        /// </summary>
        /// <param name="taskId">The task ID</param>
        /// <param name="taskEffortPoints">Total effort points for the task</param>
        /// <returns>List of TaskAssignee entities with updated effort points</returns>
        public async Task<List<TaskAssignee>> DistributeEffortPointsAutomatically(Guid taskId, int? taskEffortPoints)
        {
            if (!taskEffortPoints.HasValue || taskEffortPoints.Value <= 0)
            {
                return new List<TaskAssignee>();
            }

            var assignees = await _taskAssigneeRepository.taskAssigneesAsync(taskId);
            if (assignees.Count == 0)
            {
                return new List<TaskAssignee>();
            }

            var distribution = DistributeEffortPointsEqually(taskEffortPoints.Value, assignees.Count);
            
            for (int i = 0; i < assignees.Count; i++)
            {
                assignees[i].AssignedEffortPoints = distribution[i];
            }

            return assignees;
        }

        /// <summary>
        /// Redistributes effort points when an assignee leaves a task
        /// </summary>
        /// <param name="taskId">The task ID</param>
        /// <param name="taskEffortPoints">Total effort points for the task</param>
        /// <returns>List of remaining TaskAssignee entities with redistributed effort points</returns>
        public async Task<List<TaskAssignee>> RedistributeEffortPointsAfterAssigneeLeaves(Guid taskId, int? taskEffortPoints)
        {
            if (!taskEffortPoints.HasValue || taskEffortPoints.Value <= 0)
            {
                return new List<TaskAssignee>();
            }

            var remainingAssignees = await _taskAssigneeRepository.taskAssigneesAsync(taskId);
            if (remainingAssignees.Count == 0)
            {
                return new List<TaskAssignee>();
            }

            var distribution = DistributeEffortPointsEqually(taskEffortPoints.Value, remainingAssignees.Count);
            
            for (int i = 0; i < remainingAssignees.Count; i++)
            {
                remainingAssignees[i].AssignedEffortPoints = distribution[i];
            }

            return remainingAssignees;
        }

        /// <summary>
        /// Sets task effort points to total of assignee effort points if task doesn't have effort points
        /// </summary>
        /// <param name="taskId">The task ID</param>
        /// <returns>Total effort points that should be set for the task</returns>
        public async Task<int> CalculateTaskEffortPointsFromAssignees(Guid taskId)
        {
            var assignees = await _taskAssigneeRepository.taskAssigneesAsync(taskId);
            return assignees.Sum(a => a.AssignedEffortPoints ?? 0);
        }

        /// <summary>
        /// Checks if effort points need to be redistributed after assignment changes
        /// </summary>
        /// <param name="taskId">The task ID</param>
        /// <param name="taskEffortPoints">Current task effort points</param>
        /// <returns>True if redistribution is needed</returns>
        public async Task<bool> NeedsEffortPointsRedistribution(Guid taskId, int? taskEffortPoints)
        {
            if (!taskEffortPoints.HasValue)
            {
                return false;
            }

            var assignees = await _taskAssigneeRepository.taskAssigneesAsync(taskId);
            if (assignees.Count <= 1)
            {
                return false;
            }

            var totalAssigned = assignees.Sum(a => a.AssignedEffortPoints ?? 0);
            return totalAssigned != taskEffortPoints.Value;
        }
    }
}

