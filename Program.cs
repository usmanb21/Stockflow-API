using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Keep your port binding (DO NOT CHANGE)
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// ======================
// Services
// ======================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ======================
// Swagger (NO OAuth for now)
// ======================

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "inventory-cloud-api",
        Version = "v1"
    });
});

// ======================
// Authentication (RESTORED)
// ======================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

// ======================
// Build
// ======================

var app = builder.Build();

// ======================
// Middleware
// ======================

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();   // ✅ MUST be before Authorization
app.UseAuthorization();

// ======================
// Endpoints
// ======================

app.MapControllers();

// ✅ Public endpoints (keep working for Azure health check)
app.MapGet("/", () => "StockFlow API is running").AllowAnonymous();
app.MapGet("/health", () => Results.Ok("Healthy")).AllowAnonymous();

// ======================
// Run
// ======================

app.Run();