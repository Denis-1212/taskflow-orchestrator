namespace TaskFlow.Services.Task.Domain;

using System.Runtime.Serialization;

public enum TaskItemStatus
{
    [EnumMember(Value = "Todo")]
    Todo = 0,

    [EnumMember(Value = "InProgress")]
    InProgress = 1,

    [EnumMember(Value = "Completed")]
    Completed = 2,

    [EnumMember(Value = "Cancelled")]
    Cancelled = 3
}
