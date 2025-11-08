using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Load configs
builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile("swaggerForOcelot.json", optional: false, reloadOnChange: true);

// 🔹 Register services
builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

// 🔹 Redirect "/" -> "/swagger"
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger");
        return;
    }
    await next();
});

// 🔹 Simple console logging for incoming requests
app.Use(async (context, next) =>
{
    Console.WriteLine($"➡️ Request: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"⬅️ Response: {context.Response.StatusCode}");
});

// 🔹 SwaggerForOcelot setup
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
    opt.ReConfigureUpstreamSwaggerJson = (context, json) =>
    {
        // Optional: thêm log để debug nếu service nào không hiện
        return json;
    };
});

// 🔹 Run Ocelot Gateway
await app.UseOcelot();
app.Run();
