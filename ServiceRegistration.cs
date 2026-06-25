using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineJobRecruitmentSystem.Data;
using OnlineJobRecruitmentSystem.Extensions;
using OnlineJobRecruitmentSystem.Profiles;
using OnlineJobRecruitmentSystem.Services;
using OnlineJobRecruitmentSystem.Services.Interfaces;
using System.Text;

namespace OnlineJobRecruitmentSystem
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            services.AddAutoMapper(cfg => cfg.AddProfile<MapperProfile>());
            services.AddValidatorsFromAssemblyContaining<Program>();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(
                c =>
                {
                    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = Microsoft.OpenApi.Models.ParameterLocation.Header
                    });
                    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                    {
                        {
                             new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                             {
                                 Reference = new Microsoft.OpenApi.Models.OpenApiReference
                                 {
                                     Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                     Id = "Bearer"
                                 }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<FileManager>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddHostedService<JobExpiryService>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["Jwt:Issuer"],
                        ValidAudience = configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!))
                    };
                });

            return services;
        }
    }
}
