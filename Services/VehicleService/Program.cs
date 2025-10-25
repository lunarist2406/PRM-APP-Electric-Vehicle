using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using VehicleService.Data;
using VehicleService.Services;
using VehicleService.Swagger;
using DotNetEnv; // 🧠 thêm package DotNetEnv

var builder = WebApplication.CreateBuilder(args);

// ==========================
// 🌍 Load ENV + Config
// ==========================
Env.Load(); // load .env
var config = builder.Configuration;

// ==========================
// 🔐 JWT Auth Setup
// ==========================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? "default_secret")),
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            ClockSkew = TimeSpan.Zero
        };
    });

// ==========================
// 🧩 MongoDB + DI
// ==========================
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddHttpContextAccessor(); // để service có thể lấy HttpContext
builder.Services.AddScoped<VehicleDataService>();
builder.Services.AddHttpClient(); // thêm HttpClientFactory

// ==========================
// 🚀 Controllers + Swagger
// ==========================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VehicleService API",
        Version = "v1",
        Description = "API for Vehicle Management (MongoDB + JWT Auth)"
    });

    // JWT Auth for Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo format: Bearer {token}"
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

    c.SchemaFilter<RegisterVehicleDtoExampleSchemaFilter>();
});

// ==========================
// 🚀 Build App
// ==========================
var app = builder.Build();

// ==========================
// 🌍 Middleware
// ==========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "VehicleService API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
