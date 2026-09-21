using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TerryCorner.Application.Common.Interfaces;
using TerryCorner.Infrastructure.Identity;
using TerryCorner.Infrastructure.Persistence;
using TerryCorner.Infrastructure.Services;

namespace TerryCorner.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddDbContext<TerryCornerDbContext>(options =>
        {
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), npgsqlOptions =>
            {
                // Workaround/diagnostic for a DbUpdateConcurrencyException seen with PostgreSQL 18 +
                // Npgsql's default statement batching, where a SaveChanges() touching multiple rows
                // (an UPDATE plus several INSERTs) reports an incorrect affected-row count for one of
                // the batched statements. Sending one statement per round-trip avoids it; revisit if
                // Npgsql ships a fix and this stops being necessary.
                npgsqlOptions.MaxBatchSize(1);
            });

            if (isDevelopment)
            {
                // Only in Development: puts actual parameter values into exception messages, so the
                // next unexpected EF Core error is immediately diagnosable instead of guessed at.
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<TerryCornerDbContext>());

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                // Stronger password policy per the security hardening requirements.
                options.Password.RequiredLength = 10;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<TerryCornerDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };

                // Allow the JWT to be supplied via query string for SignalR hub connections,
                // since browsers can't set custom headers on the WebSocket handshake.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorization();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
