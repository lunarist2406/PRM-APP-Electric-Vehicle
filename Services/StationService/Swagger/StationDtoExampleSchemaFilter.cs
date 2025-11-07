using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using StationService.DTOs;

namespace StationService.Swagger
{
    public class StationDtoExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(StationCreateDto) || context.Type == typeof(StationUpdateDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["name"] = new OpenApiString("Station A"),
                    ["address"] = new OpenApiString("123 Main Street, Ho Chi Minh City"),
                    ["latitude"] = new OpenApiDouble(10.762622),
                    ["longitude"] = new OpenApiDouble(106.660172),
                    ["powerCapacity"] = new OpenApiInteger(50),
                    ["pricePerKwh"] = new OpenApiDouble(3000),
                    ["status"] = new OpenApiString("online")
                };
            }
        }
    }
}
