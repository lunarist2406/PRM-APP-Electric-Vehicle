using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using BookingService.Models.DTOs;
using BookingService.Models.Enums;

namespace BookingService.Swagger
{
    public class BookingUpdateDtoExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(BookingUpdateDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["start_time"] = new OpenApiString("2025-11-02T13:00:00.000Z"),
                    ["end_time"] = new OpenApiString("2025-11-02T16:00:00.000Z"),
                    ["status"] = new OpenApiString("Cancelled"),
                    ["rate_type"] = new OpenApiString(ChargingRateType.Standard.ToString())
                };
            }
        }
    }
}
