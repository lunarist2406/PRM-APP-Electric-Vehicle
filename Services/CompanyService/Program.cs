using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CompanyService.Data;
using CompanyService.Services;
using CompanyService.Swagger;
using CompanyService.Utils; // <- thêm dòng này để dùng CorsSetup

var builder = WebApplication.CreateBuilder(args);

// ==========================
// ⚡ Bật CORS
// ==========================
builder.Services.AddCustomCors(); // giờ không cần truyền mảng domain


// ==========================
// 🔐 JWT Authentication
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

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<CompanyDataService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CompanyService API",
        Version = "v1",
        Description = "API for Company management"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo dạng: Bearer {token}"
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

    c.SchemaFilter<RegisterCompanyDtoExampleSchemaFilter>();
});

var app = builder.Build();

// ==========================
// 🌍 Middleware CORS + Auth
// ==========================
app.UseCustomCors(); // <- bật CORS trước Authentication
app.UseAuthentication();
app.UseAuthorization();

// ==========================
// Swagger
// ==========================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CompanyService API V1");
    c.RoutePrefix = string.Empty;
});

app.MapControllers();
app.Run();
