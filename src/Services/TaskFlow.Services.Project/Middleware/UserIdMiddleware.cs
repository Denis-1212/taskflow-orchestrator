namespace TaskFlow.Services.Project.Middleware;

using System.Security.Claims;

using Microsoft.Extensions.Primitives;

using Task = Task;

public class UserIdMiddleware(RequestDelegate next, ILogger<UserIdMiddleware> logger)
{

    #region Methods

    public async Task InvokeAsync(HttpContext context)
    {
        // Пытаемся получить user ID из заголовка (от Gateway)
        if (context.Request.Headers.TryGetValue("X-User-Id", out StringValues userIdHeader) &&
            Guid.TryParse(userIdHeader, out Guid userId))
        {
            // Создаем ClaimsIdentity и добавляем в контекст
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "Gateway");
            context.User = new ClaimsPrincipal(identity);

            logger.LogDebug("User ID extracted from header: {UserId}", userId);
        }
        else
        {
            logger.LogWarning("No X-User-Id header found in request");
        }

        await next(context);
    }

    #endregion

}
