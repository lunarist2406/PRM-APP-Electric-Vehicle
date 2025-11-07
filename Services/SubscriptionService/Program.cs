using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SubscriptionService.Data;
using SubscriptionService.Service;
using SubscriptionService.Swagger;
using SubscriptionService.Utils;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// 🌍 Load ENV + Config
// ==========================
DotNetEnv.Env.Load();
var config = builder.Configuration;

// ==========================
// ⚡ Bật CORS
// ==========================
builder.Services.AddCustomCors();

// ==========================
// 🔐 JWT Auth Setup
// ==========================
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? config["JWT_SECRET"] 
            ?? "your-super-secret-key-at-least-32-characters-long-for-hs256";
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") 
            ?? config["JWT_ISSUER"] 
            ?? "UserService";
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") 
            ?? config["JWT_AUDIENCE"] 
            ?? "UserServiceClient";


        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            ClockSkew = TimeSpan.Zero
        };

    });

builder.Services.AddAuthorization();

// ==========================
// 🧩 MongoDB + DI
// ==========================
builder.Services.AddSingleton<MongoDbContext>(sp => 
    new MongoDbContext(sp.GetRequiredService<IConfiguration>()));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SubscriptionDataService>();
builder.Services.AddScoped<BillingService>();
builder.Services.AddHttpClient();

// ==========================
// 🛡️ Rate Limiting (per IP)
// ==========================
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

// ==========================
// 🚀 Controllers + Swagger
// ==========================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SubscriptionService API",
        Version = "v1",
        Description = "API for Subscription Management (MongoDB + JWT Auth)"
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

// ==========================
// 🚀 Build App
// ==========================
var app = builder.Build();

// ==========================
// 🌍 Middleware
// ==========================
app.UseCustomCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
c.SwaggerEndpoint("/swagger/v1/swagger.json", "SubscriptionService API V1");
c.RoutePrefix = string.Empty;
});

app.MapControllers();
app.Run();

