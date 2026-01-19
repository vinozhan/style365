using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Style365.Api.HealthChecks;
using Style365.Api.Middleware;
using Style365.Application;
using Style365.Application.Common.Models;
using Style365.Infrastructure;
using Style365.Infrastructure.Data;
using System.Reflection;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    // This adds User Secrets ONLY in development
    builder.Configuration.AddUserSecrets<Program>();
}


// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/app-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

// Use Serilog as the logging provider
builder.Host.UseSerilog();



// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI Documentation
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Style365 API",
        Version = "v1",
        Description = "Enterprise eCommerce API for Style365 clothing platform",
        Contact = new OpenApiContact
        {
            Name = "Style365 Development Team",
            Email = "dev@style365.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License"
        }
    });

    // Include XML comments for better API documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Configure JWT Bearer Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Enable annotations for better documentation
    c.EnableAnnotations();
});

// Add Authentication & Authorization
var authSettings = builder.Configuration.GetSection(AuthSettings.SectionName).Get<AuthSettings>();
if (authSettings?.Cognito != null)
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = authSettings.Cognito.Authority;
        //options.Audience = authSettings.Cognito.UserPoolClientId;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authSettings.Cognito.Authority,
            ValidateAudience = false, // Cognito Access Tokens use client_id claim, not aud
            //ValidateAudience = true,
            //ValidAudience = authSettings.Cognito.UserPoolClientId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };

    });
}


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireClaim("cognito:groups", "Admin"));
    options.AddPolicy("CustomerOrAdmin", policy =>
        policy.RequireClaim("cognito:groups", "Customer", "Admin"));
});

// Enhanced CORS Configuration
builder.Services.AddCors(options =>
{
    // Development CORS policy
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",  // Next.js storefront
                "http://localhost:3001",  // React admin dashboard
                "https://localhost:3000",
                "https://localhost:3001"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("X-Request-Id", "X-Pagination-Total", "X-Pagination-Page");
    });

    // Production CORS policy
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
                "https://www.style365.com",      // Customer storefront
                "https://admin.style365.com"     // Admin dashboard
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("X-Request-Id", "X-Pagination-Total", "X-Pagination-Page");
    });

    // Default policy (most restrictive)
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://api.style365.com")
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH")
              .WithHeaders("Content-Type", "Authorization")
              .AllowCredentials();
    });
});

// Add Application services
builder.Services.AddApplication();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database")
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Style365 API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Style365 API Documentation";
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.DefaultModelsExpandDepth(-1);
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.DisplayRequestDuration();
    });
}

// Add custom middleware
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

// Use environment-specific CORS policy
var corsPolicy = app.Environment.IsDevelopment() ? "Development" : "Production";
app.UseCors(corsPolicy);

// Health checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(x => new
            {
                name = x.Key,
                status = x.Value.Status.ToString(),
                description = x.Value.Description,
                duration = x.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure proper cleanup of Serilog
try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
