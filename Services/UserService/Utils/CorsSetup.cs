using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace UserService.Utils
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
                        .AllowAnyOrigin()    // ✅ cho mọi domain
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    // .AllowCredentials(); <-- ⚠️ Không dùng AllowCredentials với AllowAnyOrigin
                });
            });
            return services;
        }

        // ⚡ Dùng middleware CORS với WebApplication
        public static WebApplication UseCustomCors(this WebApplication app)
        {
            app.UseCors(CorsPolicyName);
            return app;
        }
    }
}
