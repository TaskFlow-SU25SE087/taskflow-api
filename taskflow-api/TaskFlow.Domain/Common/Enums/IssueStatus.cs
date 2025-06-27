namespace taskflow_api.TaskFlow.Domain.Common.Enums
{
    public enum IssueStatus
    {
        Open = 0,
        InProgress = 10000,
        Resolved = 20000,
        Closed = 30000,
        Reopened = 40000,
        OnHold = 50000,
        Cancelled = 60000
    }
}
