using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using CompanyService.Models.DTOs;

namespace CompanyService.Swagger
{
    public class RegisterCompanyDtoExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(RegisterCompanyDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["name"] = new OpenApiString("GreenCharge Station Co."),
                    ["address"] = new OpenApiString("123 Nguyễn Văn Cừ, Quận 5, TP.HCM"),
                    ["contactEmail"] = new OpenApiString("support@greencharge.vn")
                };
            }
        }
    }
}
