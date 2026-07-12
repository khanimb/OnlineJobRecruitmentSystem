using OnlineJobRecruitmentSystem.API;
using OnlineJobRecruitmentSystem.API.Hubs;
using OnlineJobRecruitmentSystem.API.Middlewares;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<OnlineJobRecruitmentSystem.Infrastructure.Data.AppDbContext>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    if (!context.Users.Any(u => u.Role == OnlineJobRecruitmentSystem.Domain.Common.Roles.Admin))
    {
        context.Users.Add(new OnlineJobRecruitmentSystem.Domain.Entities.User
        {
            Username = config["SeedAdmin:Username"] ?? "admin",
            Email = config["SeedAdmin:Email"] ?? "admin@jobsystem.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(config["SeedAdmin:Password"] ?? "ChangeMe123!"),
            Role = OnlineJobRecruitmentSystem.Domain.Common.Roles.Admin,
            IsEmailVerified = true
        });
        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.MapScalarApiReference(options =>
    {
        options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
    });
}


app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("AllowConfiguredOrigin");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();
