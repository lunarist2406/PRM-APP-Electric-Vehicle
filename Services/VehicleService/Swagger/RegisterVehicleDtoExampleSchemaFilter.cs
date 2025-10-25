using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using VehicleService.Models.DTOs;

namespace VehicleService.Swagger
{
    public class RegisterVehicleDtoExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(VehicleDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["user_id"] = new OpenApiString("66f321c4b2b59f2f08a731e3"),
                    ["company_id"] = new OpenApiString("6718a20f9372df08b43a32fd"),
                    ["plate_number"] = new OpenApiString("51H-123.45"),
                    ["model"] = new OpenApiString("VinFast VF8"),
                    ["batteryCapacity"] = new OpenApiDouble(85.5)
                };
            }
        }
    }
}
