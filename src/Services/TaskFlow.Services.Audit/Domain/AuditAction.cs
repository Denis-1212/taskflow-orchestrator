namespace TaskFlow.Services.Audit.Domain;

using System.Runtime.Serialization;

public enum AuditAction
{
    [EnumMember(Value = "CREATE")]
    Create = 1,

    [EnumMember(Value = "UPDATE")]
    Update = 2,

    [EnumMember(Value = "DELETE")]
    Delete = 3,

    [EnumMember(Value = "LOGIN")]
    Login = 4,

    [EnumMember(Value = "LOGOUT")]
    Logout = 5,

    [EnumMember(Value = "ASSIGN")]
    Assign = 6,

    [EnumMember(Value = "STATUS_CHANGE")]
    StatusChange = 7,

    [EnumMember(Value = "ADD_MEMBER")]
    AddMember = 8,

    [EnumMember(Value = "REMOVE_MEMBER")]
    RemoveMember = 9,

    [EnumMember(Value = "REGISTRATION")]
    Registration = 10
}
