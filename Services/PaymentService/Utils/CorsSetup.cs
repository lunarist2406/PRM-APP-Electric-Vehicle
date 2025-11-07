using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentService.Utils
{
    public static class CorsSetup
    {
        private const string CorsPolicyName = "AllowLocalAndDeploy";

        // ⚡ Thêm service CORS
        public static IServiceCollection AddCustomCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    policy
                        .AllowAnyOrigin()   // Cho phép tất cả domain (có thể customize nếu muốn)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            return services;
        }

        // ⚡ Dùng middleware CORS
        public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app)
        {
            app.UseCors(CorsPolicyName);
            return app;
        }
    }
}
