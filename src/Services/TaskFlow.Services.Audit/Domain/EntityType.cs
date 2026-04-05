namespace TaskFlow.Services.Audit.Domain;

using System.Runtime.Serialization;

public enum EntityType
{
    [EnumMember(Value = "User")]
    User = 1,

    [EnumMember(Value = "Task")]
    Task = 2,

    [EnumMember(Value = "Project")]
    Project = 3
}
