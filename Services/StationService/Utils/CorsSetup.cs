using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace StationService    .Utils
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
                        .AllowAnyOrigin()   // cho phép tất cả domain
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
