using System.Collections.Concurrent;

namespace OnlineJobRecruitmentSystem.API.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, (int Count, DateTime ResetTime)> _requests = new();
        private const int MaxRequests = 100;
        private const int WindowSeconds = 60;

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var now = DateTime.UtcNow;

            _requests.AddOrUpdate(ip,
                (1, now.AddSeconds(WindowSeconds)),
                (key, old) =>
                {
                    if (now > old.ResetTime)
                        return (1, now.AddSeconds(WindowSeconds));
                    return (old.Count + 1, old.ResetTime);
                });

            if (_requests[ip].Count > MaxRequests)
            {
                context.Response.StatusCode = 429;
                await context.Response.WriteAsync("Too many requests. Please try again later.");
                return;
            }

            await _next(context);
        }
    }
}
