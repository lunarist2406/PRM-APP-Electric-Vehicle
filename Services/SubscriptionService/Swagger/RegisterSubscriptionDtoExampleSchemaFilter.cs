using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;
using SubscriptionService.Model.DTOs;

namespace SubscriptionService.Swagger
{
    public class RegisterSubscriptionDtoExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(RegisterSubscriptionDto))
            {
                schema.Example = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["vehicleId"] = new Microsoft.OpenApi.Any.OpenApiString("vehicle123"),
                    ["subscriptionPlanId"] = new Microsoft.OpenApi.Any.OpenApiString("plan123"),
                    ["autoRenew"] = new Microsoft.OpenApi.Any.OpenApiBoolean(true)
                };
            }
        }
    }
}

