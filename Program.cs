using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OnlineJobRecruitmentSystem;
using OnlineJobRecruitmentSystem.Data;
using OnlineJobRecruitmentSystem.Profiles;
using OnlineJobRecruitmentSystem.Services;
using OnlineJobRecruitmentSystem.Services.Interfaces;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddApplicationServices(builder.Configuration);


var app = builder.Build();

app.UseSwagger();
app.MapScalarApiReference(options =>
{
    options.WithOpenApiRoutePattern("/swagger/v1/swagger.json");
});


app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
