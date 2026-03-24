namespace TaskFlow.Shared.Kernel;

public static class Guard
{
    public static void AgainstNull<T>(T value, string parameterName) where T : class
    {
        if (value is null)
            throw new ArgumentNullException(parameterName);
    }
    
    public static void AgainstNullOrEmpty(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{parameterName} cannot be null or empty", parameterName);
    }
    
    public static void AgainstCondition(bool condition, string message)
    {
        if (condition)
            throw new InvalidOperationException(message);
    }
}
