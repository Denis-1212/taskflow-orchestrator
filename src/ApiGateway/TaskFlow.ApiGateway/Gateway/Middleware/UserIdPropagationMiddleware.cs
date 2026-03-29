namespace TaskFlow.ApiGateway.Gateway.Middleware;

using System.Security.Claims;

public class UserIdPropagationMiddleware(RequestDelegate next, ILogger<UserIdPropagationMiddleware> logger)
{

    #region Methods

    public async Task InvokeAsync(HttpContext context)
    {
        // Извлекаем user ID из аутентифицированного пользователя
        string? userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Добавляем заголовок для внутренних сервисов
            context.Request.Headers.Append("X-User-Id", userId);
            logger.LogDebug("Propagating user ID: {UserId} to downstream service", userId);
        }

        await next(context);
    }

    #endregion

}
