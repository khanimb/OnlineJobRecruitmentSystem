using OnlineJobRecruitmentSystem;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();

app.UseSwagger();
app.MapScalarApiReference(options =>
{
    options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
});


app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();
//app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
