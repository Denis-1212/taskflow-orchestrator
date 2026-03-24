namespace TaskFlow.Shared.Kernel;

public record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
    
    public static Error NotFound(string entity, object id) => new(
        "Error.NotFound",
        $"{entity} with id '{id}' was not found",
        ErrorType.NotFound);
        
    public static Error Validation(string message) => new(
        "Error.Validation",
        message,
        ErrorType.Validation);
        
    public static Error Conflict(string message) => new(
        "Error.Conflict",
        message,
        ErrorType.Conflict);
        
    public static Error Unauthorized(string message = "Unauthorized access") => new(
        "Error.Unauthorized",
        message,
        ErrorType.Unauthorized);
        
    public static Error Forbidden(string message = "Access forbidden") => new(
        "Error.Forbidden",
        message,
        ErrorType.Forbidden);
}

public enum ErrorType
{
    None,
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden
}
