using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Repositories;
using TaskFlow.Application.Commands.CreateTask;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.ValueObjects;
using AspNetCoreRateLimit;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
    .Enrich.WithProperty("MachineName", Environment.MachineName)
    .Enrich.WithProperty("ProcessId", Environment.ProcessId)
    .Enrich.WithProperty("ThreadId", Environment.CurrentManagedThreadId)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/taskflow-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .WriteTo.Seq("http://seq:5341")
    .CreateLogger();

try
{
    Log.Information("Starting TaskFlow API");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "TaskFlow API",
            Version = "v1",
            Description = "A production-ready .NET 8 API implementing Domain-Driven Design",
            Contact = new OpenApiContact
            {
                Name = "TaskFlow Team",
                Email = "api@taskflow.com"
            }
        });

        // Include XML comments
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        c.EnableAnnotations();
    });

    // Add Entity Framework
    builder.Services.AddDbContext<TaskFlowDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Add Identity services
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<TaskFlowDbContext>()
    .AddDefaultTokenProviders();

    // Configure Google OAuth
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
            options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
            options.UsePkce = true;
            options.CallbackPath = "/api/auth/google-callback";
            
            // Configure scopes
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");
            
            // For development, allow HTTP and configure cookie settings
            if (builder.Environment.IsDevelopment())
            {
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.None;
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.HttpOnly = false;
            }
            
            // Configure events for better error handling
            options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
            {
                OnRemoteFailure = context =>
                {
                    Log.Warning("Google OAuth remote failure: {Error}", context.Failure?.Message);
                    context.Response.Redirect("/api/auth/google-login?error=oauth_failure");
                    context.HandleResponse();
                    return Task.CompletedTask;
                }
            };
        });

    // Add MediatR
    builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateTaskCommand).Assembly));

    // Add Repositories - Following DDD principles with proper dependency injection
    builder.Services.AddScoped<ITaskRepository, TaskRepository>();
    
    builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
    builder.Services.AddScoped<IUnitOfWork, TaskFlowDbContext>();

    // Add Health Checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<TaskFlowDbContext>("Database")
        .AddCheck("Self", () => HealthCheckResult.Healthy("API is healthy"));

    // Add CORS with secure configuration
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("SecureCors", policy =>
        {
            policy
                .WithOrigins(
                    "https://app.taskflow.com", 
                    "https://admin.taskflow.com", 
                    "http://localhost:3000", 
                    "https://localhost:3000",
                    "http://localhost:7001",
                    "https://localhost:7001",
                    "http://localhost:8000",
                    "https://localhost:8000"
                )
                .AllowCredentials()
                .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                .WithHeaders("Authorization", "Content-Type", "X-Requested-With")
                .WithExposedHeaders("X-RateLimit-Limit", "X-RateLimit-Remaining", "X-RateLimit-Reset");
        });

        options.AddPolicy("PublicApi", policy =>
        {
            policy
                .WithOrigins("*")
                .WithMethods("GET")
                .WithHeaders("Content-Type");
        });
    });

    // Add rate limiting
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(options =>
    {
        options.EnableEndpointRateLimiting = true;
        options.StackBlockedRequests = false;
        options.GeneralRules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Endpoint = "*",
                Limit = 100,
                Period = "1m"
            },
            new RateLimitRule
            {
                Endpoint = "*",
                Limit = 1000,
                Period = "1h"
            }
        };
    });

    builder.Services.Configure<ClientRateLimitOptions>(options =>
    {
        options.EnableEndpointRateLimiting = true;
        options.StackBlockedRequests = false;
        options.GeneralRules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Endpoint = "*",
                Limit = 50,
                Period = "1m"
            }
        };
    });

    builder.Services.AddInMemoryRateLimiting();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    // Add Authentication (JWT)
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            var jwtSettings = builder.Configuration.GetSection("Authentication:Jwt");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    // Add Authorization
    builder.Services.AddAuthorization();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API v1");
            c.RoutePrefix = "swagger";
        });
    }

    // Add comprehensive security headers middleware
    app.Use(async (context, next) =>
    {
        // Basic security headers
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        context.Response.Headers["X-Frame-Options"] = "DENY";
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["X-Download-Options"] = "noopen";
        context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
        context.Response.Headers["X-DNS-Prefetch-Control"] = "off";
        
        // Permissions Policy
        context.Response.Headers["Permissions-Policy"] = string.Join(", ", new[]
        {
            "geolocation=()",
            "microphone=()",
            "camera=()",
            "payment=()",
            "usb=()",
            "magnetometer=()",
            "gyroscope=()",
            "accelerometer=()",
            "ambient-light-sensor=()",
            "autoplay=()",
            "encrypted-media=()",
            "fullscreen=()",
            "picture-in-picture=()",
            "sync-xhr=()"
        });
        
        // Content Security Policy
        context.Response.Headers["Content-Security-Policy"] = 
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self'; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self';";
        
        // Add HSTS header for HTTPS requests
        if (context.Request.IsHttps)
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
        }
        
        await next();
    });

    app.UseHttpsRedirection();

    // Add rate limiting middleware
    app.UseIpRateLimiting();
    app.UseClientRateLimiting();

    app.UseCors("SecureCors");

    app.UseAuthentication();
    app.UseAuthorization();

    // Add health checks endpoint
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow.ToString("O"),
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    duration = entry.Value.Duration.TotalMilliseconds
                })
            };
            await context.Response.WriteAsJsonAsync(response);
        }
    });

    app.MapControllers();

    // Ensure database is created
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<TaskFlowDbContext>();
        context.Database.EnsureCreated();
        Log.Information("Database created successfully");
    }

    Log.Information("TaskFlow API started successfully");

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