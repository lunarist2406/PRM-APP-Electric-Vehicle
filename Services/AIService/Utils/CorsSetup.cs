using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AIService.Utils
{
    public static class CorsSetup
    {
        private const string CorsPolicyName = "AllowAiServiceClients";
        public static IServiceCollection AddCustomCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    policy
                        .AllowAnyOrigin()   
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            return services;
        }
        public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app)
        {
            app.UseCors(CorsPolicyName);
            return app;
        }
    }
}
