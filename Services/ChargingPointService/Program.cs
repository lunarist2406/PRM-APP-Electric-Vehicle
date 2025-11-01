using System.Text;
using System.Threading.RateLimiting;
using ChargingPointService.Data;
using ChargingPointService.Services;
using ChargingPointService.Utils;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 🌍 Load .env
Env.Load();
builder.Configuration.AddEnvironmentVariables();

Console.WriteLine("========== 🌍 ENV CHECK ==========");
Console.WriteLine($"📁 Current Directory: {Directory.GetCurrentDirectory()}");
Console.WriteLine($"✅ MONGO_URI: {Environment.GetEnvironmentVariable("MONGO_URI")}");
Console.WriteLine($"✅ MONGO_DB_NAME: {Environment.GetEnvironmentVariable("MONGO_DB_NAME")}");
Console.WriteLine($"✅ JWT_SECRET: {Environment.GetEnvironmentVariable("JWT_SECRET")}");
Console.WriteLine($"✅ STATION_API_URL: {Environment.GetEnvironmentVariable("STATION_API_URL")}");
Console.WriteLine("==================================");
// ============================================
// 🔐 JWT Authentication
// ============================================
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
            PermitLimit = 20,
            Window = TimeSpan.FromSeconds(10),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });

    options.OnRejected = (context, token) =>
    {
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"🚫 IP {ip} bị chặn vì spam quá nhanh (Rate Limit)!");
        Console.ResetColor();
        context.HttpContext.Response.Headers["Retry-After"] = "10";
        return ValueTask.CompletedTask;
    };

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ============================================
// 📦 Dependency Injection
// ============================================

// MongoDbContext (singleton)
builder.Services.AddSingleton<MongoDbContext>();

// HttpClient + HttpContextAccessor
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// App services
builder.Services.AddScoped<ChargingPointApiService>();

// ============================================
// 📘 Swagger
// ============================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "⚡ ChargingPointService API",
        Version = "v1",
        Description = "API quản lý trạm sạc điện (EV) có tích hợp JWT + Rate Limiting"
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
    c.SchemaFilter<ChargingPointService.Swagger.ChargingPointDtoExampleSchemaFilter>();

});

// ============================================
// 🚀 Build App
// ============================================
var app = builder.Build();

// ✅ Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChargingPointService API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCustomCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
