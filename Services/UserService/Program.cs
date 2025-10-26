using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using System.Text;
using UserService.Data;
using UserService.Services;
using UserService.Utils; // <- thêm dòng này để dùng CorsSetup
using UserService.Swagger;

// ⚠️ Bật log chi tiết JWT
IdentityModelEventSource.ShowPII = true;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// 🧩 MongoDB DI + Connection check
// ==========================
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<UserService.Services.UserService>();
builder.Services.AddScoped<JwtService>();

// Test MongoDB connection
try
{
    var mongoUri = builder.Configuration["MONGO_URI"];
    if (string.IsNullOrEmpty(mongoUri))
        throw new Exception("Missing MongoDB connection string in appsettings!");

    var testClient = new MongoClient(mongoUri);
    var dbList = testClient.ListDatabaseNames().ToList();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("✅ Connected to MongoDB successfully!");
    Console.ResetColor();

    Console.WriteLine("📚 Available databases:");
    foreach (var db in dbList) Console.WriteLine($"   - {db}");
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ MongoDB connection failed: {ex.Message}");
    Console.ResetColor();
}

// ==========================
// ⚡ Bật CORS
// ==========================
builder.Services.AddCustomCors(); // giờ không cần truyền mảng domain


// ==========================
// 🔐 JWT Authentication (HS256)
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
                Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"]!)),
            ValidIssuer = builder.Configuration["JWT_ISSUER"],
            ValidAudience = builder.Configuration["JWT_AUDIENCE"],
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    Console.WriteLine($"➡️ Incoming token (trimmed Bearer): {context.Token}");
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("❌ Authentication failed:");
                Console.WriteLine(context.Exception.GetType().FullName);
                Console.WriteLine(context.Exception.Message);
                if (context.Exception.InnerException != null)
                    Console.WriteLine("Inner: " + context.Exception.InnerException.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("✅ Token validated!");
                foreach (var c in context.Principal.Claims)
                    Console.WriteLine($"Claim: {c.Type} = {c.Value}");
                return Task.CompletedTask;
            }
        };
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
        Title = "UserService API",
        Version = "v1",
        Description = "API for User Management with MongoDB + JWT Authentication"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });

    c.EnableAnnotations();
    c.SchemaFilter<RegisterDtoExampleSchemaFilter>();
    c.SchemaFilter<LoginDtoExampleSchemaFilter>();
});

var app = builder.Build();

// ==========================
// 🌍 Middleware CORS + Auth
// ==========================
app.UseCustomCors(); // <- bật CORS đầu tiên
app.UseAuthentication(); // ⚠️ phải trước Authorization

// 🔍 Debug middleware token + claims
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    Console.WriteLine($"➡️ Incoming token header: {authHeader}");

    if (context.User.Identity?.IsAuthenticated == true)
    {
        Console.WriteLine("✅ Authenticated User Claims:");
        foreach (var c in context.User.Claims)
            Console.WriteLine($"Type: {c.Type}, Value: {c.Value}");
    }
    else
    {
        Console.WriteLine("❌ User not authenticated");
    }

    await next.Invoke();
});

app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
    c.RoutePrefix = string.Empty;
});

app.MapControllers();
app.Run();
