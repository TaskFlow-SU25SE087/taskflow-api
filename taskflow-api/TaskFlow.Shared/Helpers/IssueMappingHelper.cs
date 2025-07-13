using taskflow_api.TaskFlow.Domain.Common.Enums;

namespace taskflow_api.TaskFlow.Shared.Helpers
{
    public class IssueMappingHelper
    {
        public static TaskPriority MapSeverityToPriority(string severity)
        {
            return severity switch
            {
                "BLOCKER" => TaskPriority.High,
                "CRITICAL" => TaskPriority.Medium,
                _ => TaskPriority.Low,
            };
        }
    }
}
