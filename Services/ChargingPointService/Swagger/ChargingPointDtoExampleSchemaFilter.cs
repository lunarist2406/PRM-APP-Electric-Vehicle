using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using ChargingPointService.Models.DTOs;

namespace ChargingPointService.Swagger
{
    public class ChargingPointDtoExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // Example cho ChargingPointCreateDto
            if (context.Type == typeof(ChargingPointCreateDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["stationId"] = new OpenApiString("69024d0d48176f388ef6012f"),
                    ["PointName"] = new OpenApiString("ABC"),
                    ["type"] = new OpenApiString("Online"),
                    ["status"] = new OpenApiString("available")
                };
            }

            // Example cho ChargingPointUpdateDto
            if (context.Type == typeof(ChargingPointUpdateDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["stationId"] = new OpenApiString("ST001"),
                    ["PointName"] = new OpenApiString("ACB"),
                    ["type"] = new OpenApiString("Offline"),
                    ["status"] = new OpenApiString("maintenance")
                };
            }
        }
    }
}
