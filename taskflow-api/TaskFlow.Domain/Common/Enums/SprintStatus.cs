namespace taskflow_api.TaskFlow.Domain.Common.Enums
{
    public enum SprintStatus
    {
        NotStarted = 0,
        InProgress = 10000,
        Completed = 20000,
        OnHold = 30000,
        Cancelled =  40000,
    }
}
