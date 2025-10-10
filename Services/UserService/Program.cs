using UserService.Data;
using UserService.Services;
using UserService.Utils;
using Microsoft.OpenApi.Models;   // Cho SwaggerDoc
var builder = WebApplication.CreateBuilder(args);

// MongoDB DI
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<UserService.Services.UserService>(); 
builder.Services.AddScoped<JwtService>();

// JWT Authentication
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
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT_SECRET"]!)) // dấu ! để loại bỏ cảnh báo null
        };
    });

// Add controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserService API", Version = "v1" });
});

var app = builder.Build();

// Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
        c.RoutePrefix = string.Empty; // Truy cập http://localhost:5080 là thấy Swagger UI
    });
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
