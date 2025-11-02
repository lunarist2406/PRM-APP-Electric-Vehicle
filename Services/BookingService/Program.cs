using System.Text;
using System.Threading.RateLimiting;
using BookingService.Data;
using BookingService.Repositories;
using BookingService.Services;
using BookingService.External;
using BookingService.Utils;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

// 🌍 Load .env file
Env.Load();
builder.Configuration.AddEnvironmentVariables();

Console.WriteLine("========== 🌍 ENV CHECK ==========");
Console.WriteLine($"📁 Current Directory: {Directory.GetCurrentDirectory()}");
Console.WriteLine($"✅ MONGO_URI: {Environment.GetEnvironmentVariable("MONGO_URI")}");
Console.WriteLine($"✅ MONGO_DB_NAME: {Environment.GetEnvironmentVariable("MONGO_DB_NAME")}");
Console.WriteLine($"✅ JWT_SECRET: {Environment.GetEnvironmentVariable("JWT_SECRET")}");
Console.WriteLine($"✅ STATION_API_URL: {Environment.GetEnvironmentVariable("STATION_API_URL")}");
Console.WriteLine($"✅ VEHICLE_API_URL: {Environment.GetEnvironmentVariable("VEHICLE_API_URL")}");
Console.WriteLine($"✅ USER_API_URL: {Environment.GetEnvironmentVariable("USER_API_URL")}");
Console.WriteLine($"✅ CHARGING_POINT_API_URL: {Environment.GetEnvironmentVariable("CHARGINGPOINT_API_URL")}");
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

// MongoDbContext
builder.Services.AddSingleton<MongoDbContext>();

// HttpClient + HttpContextAccessor
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// External clients (có gửi token)
builder.Services.AddScoped<StationClient>();
builder.Services.AddScoped<UserClient>();
builder.Services.AddScoped<VehicleClient>();
builder.Services.AddScoped<ChargingPointClient>();

// Repository + Service
// Repository + Service
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<BookingServiceLayer>();


// ============================================
// 📘 Swagger
// ============================================
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "⚡ BookingService API",
        Version = "v1",
        Description = "API quản lý booking (đặt lịch sạc) có JWT + Rate Limiting + External Services"
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

// ============================================
// 🚀 Build App
// ============================================
var app = builder.Build();

// ✅ Middleware pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BookingService API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCustomCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
