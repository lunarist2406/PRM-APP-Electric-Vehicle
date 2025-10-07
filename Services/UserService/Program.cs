using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UserService.Data;
using UserService.Repositories;
using UserService.Services;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=UserDb.db"));

// 2️⃣ Add Repository & Service
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService.Services.UserService>();

// 3️⃣ Add Controllers
builder.Services.AddControllers();

// 4️⃣ Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UserService API", Version = "v1" });
});

var app = builder.Build();

// 5️⃣ Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UserService API V1");
    });
}

// 6️⃣ HTTPS + Authorization
//app.UseHttpsRedirection();
app.UseAuthorization();

// 7️⃣ Map Controllers
app.MapControllers();

// 8️⃣ Run app
app.Run();
