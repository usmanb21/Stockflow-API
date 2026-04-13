using Microsoft.EntityFrameworkCore;
using inventory_cloud_api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================
// Services
// ============================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ============================
// Swagger + OAuth
// ============================

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "inventory-cloud-api",
        Version = "v1"
    });

    var tenantId = builder.Configuration["AzureAd:TenantId"];

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
            new[] { "api://b72268e4-116d-4f04-b4a1-cd6a95567bd7/.default" }
        }
    });
});

// ============================
// Database
// ============================

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    })
);

// ============================
// Authentication
// ============================

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

// ============================
// Build
// ============================

var app = builder.Build();

// ============================
// Auto migrations
// ============================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception)
    {
        Console.WriteLine("DB not available, skipping migration");
    }
}

// ============================
// Swagger UI
// ============================

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.OAuthClientId("f561c4ef-a529-4adb-84b2-a3bb96a61e86");
    options.OAuthUsePkce();
});

// ============================
// Middleware
// ============================

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "StockFlow API is running");

app.Run();