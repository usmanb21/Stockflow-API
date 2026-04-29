using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Identity.Web;
using Microsoft.EntityFrameworkCore;
using inventory_cloud_api.Data;
using inventory_cloud_api.Middleware;
using Serilog;

// FORCE REBUILD 2026-04-26
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:InstrumentationKey"]);


// Add services
builder.Services.AddControllers();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://login.microsoftonline.com/3c21b5f0-0af8-4d57-a2bf-ba61c165cfd7/oauth2/v2.0/authorize"),
                TokenUrl = new Uri("https://login.microsoftonline.com/3c21b5f0-0af8-4d57-a2bf-ba61c165cfd7/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "api://b72268e4-116d-4f04-b4a1-cd6a95567bd7/access_as_user", "Access API" }
                }
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "api://b72268e4-116d-4f04-b4a1-cd6a95567bd7/access_as_user" }
        }
    });
});

// Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

// Health checks
builder.Services.AddHealthChecks()
    .AddSqlServer(
        builder.Configuration
            .GetConnectionString("DefaultConnection")!
    );

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseMiddleware<ExceptionMiddleware>();

app.UseSwaggerUI(options =>
{
    options.OAuthClientId("f561c4ef-a529-4adb-84b2-a3bb96a61e86");
    options.OAuthUsePkce();
    options.OAuthScopes("api://b72268e4-116d-4f04-b4a1-cd6a95567bd7/access_as_user");
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapGet("/", () => "RUNNING OK").AllowAnonymous();

app.Run();