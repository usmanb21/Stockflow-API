using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ KEEP this (Azure container port)
builder.WebHost.UseUrls("http://0.0.0.0:8080");

// ======================
// Services
// ======================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ======================
// Swagger + JWT Auth
// ======================

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "inventory-cloud-api",
        Version = "v1"
    });

    // 🔐 JWT Auth in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});

// ======================
// Authentication
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

app.UseAuthentication();   // ⚠️ MUST be before Authorization
app.UseAuthorization();

// ======================
// Endpoints
// ======================

app.MapControllers();

// ✅ Public endpoints
app.MapGet("/", () => "StockFlow API is running").AllowAnonymous();
app.MapGet("/health", () => Results.Ok("Healthy")).AllowAnonymous();

// ======================
// Run
// ======================

app.Run();