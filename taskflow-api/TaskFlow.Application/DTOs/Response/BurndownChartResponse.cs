using taskflow_api.TaskFlow.Domain.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace taskflow_api.TaskFlow.Application.DTOs.Response
{
    public class BurndownChartResponse
    {
        public Guid SprintId { get; set; }
        public string SprintName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public List<PriorityEffortData> PriorityEfforts { get; set; } = new List<PriorityEffortData>();
        public List<DailyProgressData> DailyProgress { get; set; } = new List<DailyProgressData>();
        public List<DailyProgressData> IdealBurndown { get; set; } = new List<DailyProgressData>();
    }

    public class PriorityEffortData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public TaskPriority Priority { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public int TotalEffortPoints { get; set; }
        public int CompletedEffortPoints { get; set; }
        public int RemainingEffortPoints { get; set; }
        public double CompletionPercentage { get; set; }
    }

    public class DailyProgressData
    {
        public DateTime Date { get; set; }
        public int RemainingEffortPoints { get; set; }
        public int CompletedEffortPoints { get; set; }
        public int TotalEffortPoints { get; set; }
    }
} 