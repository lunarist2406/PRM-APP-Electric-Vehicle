using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using AIService.Data;
using AIService.External;
using AIService.Repositories;
using AIService.Services;
using AIService.Utils;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔥 Load .env
Env.Load();
builder.Configuration.AddEnvironmentVariables();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "default_secret";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            ClockSkew = TimeSpan.Zero
        };
    });

// ============================================
// 🌐 CORS + Rate Limiting
// ============================================
builder.Services.AddCustomCors();

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 25,
            Window = TimeSpan.FromSeconds(10),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });

    options.OnRejected = (context, token) =>
    {
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"IP {ip} bị chặn vì spam quá nhanh (Rate Limit)!");
        Console.ResetColor();
        context.HttpContext.Response.Headers["Retry-After"] = "10";
        return ValueTask.CompletedTask;
    };

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ============================================
// 🔌 Services / DI
// ============================================
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// External Clients
builder.Services.AddScoped<StationClient>();

// Repository + Service Layer
builder.Services.AddScoped<ChatHistoryRepository>();
builder.Services.AddScoped<AiServiceLayer>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AIService API",
        Version = "v1",
        Description = "API quản lý chat AI, lưu lịch sử chat, truy vấn trạm (Gemini + StationService)"
    });

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
});

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AIService API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCustomCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
