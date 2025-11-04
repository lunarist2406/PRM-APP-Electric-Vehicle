using System.Text;
using System.Threading.RateLimiting;
using BookingService.Data;
using BookingService.Swagger;
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


builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<StationClient>();
builder.Services.AddScoped<UserClient>();
builder.Services.AddScoped<VehicleClient>();
builder.Services.AddScoped<ChargingPointClient>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<BookingServiceLayer>();

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
        Title = "BookingService API",
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
    c.SchemaFilter<BookingCreateDtoExampleSchemaFilter>();
    c.SchemaFilter<BookingUpdateDtoExampleSchemaFilter>();
});


var app = builder.Build();
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
