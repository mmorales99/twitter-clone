using AutoRegister;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using twitter_clone_api.utils;

namespace twitter_clone_api;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();
        try 
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(builder.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            });
            builder.Services.AddSerilog((services, lc) => lc
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console());

            builder.AddMongoDBClient(connectionName: "mongodb");

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Twitter Clone API",
                    Version = "v1",
                    Description = "API para sistema de redes sociales tipo Twitter"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT encriptado",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });
            });

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddAutoregister(Assembly.GetExecutingAssembly());
            builder.Services.AddValidators();
            builder.Services.AddHostedService<TokenCleanupService>();

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
                        TokenDecryptionKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:EncryptionKey"]!)),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        RequireSignedTokens = true,
                        ClockSkew = TimeSpan.FromSeconds(15),
                        IgnoreTrailingSlashWhenValidatingAudience = true,
                        LogValidationExceptions = true,
                        SaveSigninToken = true
                    };
                    options.TokenHandlers.Clear();
                    options.TokenHandlers.Add(new JsonWebTokenHandler() {
                        TokenLifetimeInMinutes = 60,
                    });
                });
            builder.Services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin").RequireClaim("is_admin", "true"));

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
            });
            builder.Services.AddRateLimiter(options => {
                options.AddPolicy("login", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.User.Identity?.Name ?? "anon",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            Window = TimeSpan.FromMinutes(1),
                            PermitLimit = 5
                        }));
            });

            builder.Services
                .AddHealthChecks()
                .AddMongoDb(builder.Configuration.GetConnectionString("mongodb")!, name: "mongodb");
            builder.Services.AddHealthChecksUI().AddInMemoryStorage();

            var app = builder.Build();
            app.UseSerilogRequestLogging();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            // Headers de seguridad
            app.UseHsts();
            app.Use(async (context, next) => {
                context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
                await next();
            });

            // CORS específico
            app.UseCors(policy => policy
                .WithOrigins("https://tusitio.com")
                .AllowAnyMethod()
                .AllowAnyHeader());


            app.UseHttpsRedirection();
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.MapHealthChecksUI();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
