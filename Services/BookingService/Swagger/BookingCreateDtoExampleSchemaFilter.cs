using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using BookingService.Models.DTOs;
using BookingService.Models.Enums;

namespace BookingService.Swagger
{
    public class BookingCreateDtoExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(BookingCreateDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["user_id"] = new OpenApiString("68f79dbf6804e9e2b15b47b7"),
                    ["station_id"] = new OpenApiString("69024d0d48176f388ef6012f"),
                    ["vehicle_id"] = new OpenApiString("68fe27e48b2376ca7938e2ed"),
                    ["chargingPoint_id"] = new OpenApiString("690573b2e85eaa3487155172"),
                    ["start_time"] = new OpenApiString("2025-11-02T11:18:16.264Z"),
                    ["end_time"] = new OpenApiString("2025-11-02T17:04:06.346Z"),
                    ["rate_type"] = new OpenApiString(ChargingRateType.Standard.ToString())
                };
            }
        }
    }
}
