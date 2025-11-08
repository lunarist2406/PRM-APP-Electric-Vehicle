using DotNetEnv;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using System.Text.Json;

// 🔹 Load environment variables
Env.Load(); // Load từ file .env

var builder = WebApplication.CreateBuilder(args);

// 🔹 Load Ocelot + Swagger configs
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile("swaggerForOcelot.json", optional: false, reloadOnChange: true);

// 🔹 Replace placeholders trong swaggerForOcelot.json bằng biến môi trường
var swaggerFilePath = Path.Combine(builder.Environment.ContentRootPath, "swaggerForOcelot.json");
var swaggerJson = File.ReadAllText(swaggerFilePath);

// List các service keys và env vars
var serviceEnvMap = new Dictionary<string, string>
{
    ["${USER_SERVICE_URL}"] = Environment.GetEnvironmentVariable("USER_SERVICE_URL")!,
    ["${VEHICLE_SERVICE_URL}"] = Environment.GetEnvironmentVariable("VEHICLE_SERVICE_URL")!,
    ["${SUBSCRIPTION_SERVICE_URL}"] = Environment.GetEnvironmentVariable("SUBSCRIPTION_SERVICE_URL")!,
    ["${STATION_SERVICE_URL}"] = Environment.GetEnvironmentVariable("STATION_SERVICE_URL")!,
    ["${COMPANY_SERVICE_URL}"] = Environment.GetEnvironmentVariable("COMPANY_SERVICE_URL")!,
    ["${CHARGING_POINT_SERVICE_URL}"] = Environment.GetEnvironmentVariable("CHARGING_POINT_SERVICE_URL")!,
    ["${BOOKING_SERVICE_URL}"] = Environment.GetEnvironmentVariable("BOOKING_SERVICE_URL")!,
    ["${PAYMENT_SERVICE_URL}"] = Environment.GetEnvironmentVariable("PAYMENT_SERVICE_URL")!,
    ["${AI_SERVICE_URL}"] = Environment.GetEnvironmentVariable("AI_SERVICE_URL")!
};

foreach (var kv in serviceEnvMap)
{
    swaggerJson = swaggerJson.Replace(kv.Key, kv.Value);
}

// Lưu lại JSON tạm (hoặc chỉ dùng swaggerJson trực tiếp khi đăng ký)
File.WriteAllText(swaggerFilePath, swaggerJson);

// 🔹 Register Ocelot & SwaggerForOcelot
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

// 🔹 Redirect root "/" -> Swagger UI
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger");
        return;
    }
    await next();
});

// 🔹 Logging requests
app.Use(async (context, next) =>
{
    Console.WriteLine($"➡️ Request: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"⬅️ Response: {context.Response.StatusCode}");
});

// 🔹 SwaggerForOcelot UI setup
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
    // Xóa hẳn ReConfigureUpstreamSwaggerJson hoặc để trả thẳng json
    opt.ReConfigureUpstreamSwaggerJson = (context, json) => json;
});


// 🔹 Run Ocelot
await app.UseOcelot();
app.Run();
