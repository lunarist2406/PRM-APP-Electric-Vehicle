using UserService.Data;
using UserService.Services;
using UserService.Utils;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// 🧩  MongoDB DI + Connection check
// ==========================
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<UserService.Services.UserService>();
builder.Services.AddScoped<JwtService>();

// Check MongoDB connection on startup
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
    foreach (var db in dbList)
    {
        Console.WriteLine($"   - {db}");
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"❌ MongoDB connection failed: {ex.Message}");
    Console.ResetColor();
}

// ==========================
// 🔐 JWT Authentication
// ==========================
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT_ISSUER"],
            ValidAudience = builder.Configuration["JWT_AUDIENCE"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"]!))
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
});

var app = builder.Build();

// ==========================
// 🌐 Middleware
// ==========================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
