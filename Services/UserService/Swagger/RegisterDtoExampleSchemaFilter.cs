using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserService.Models.DTOs;

namespace UserService.Swagger
{
    public class RegisterDtoExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(RegisterDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["name"] = new OpenApiString("Nguyen Van A"),
                    ["email"] = new OpenApiString("nguyenvana@gmail.com"),
                    ["phone"] = new OpenApiString("+84987654321"),
                    ["password"] = new OpenApiString("123456")
                };
            }
        }
    }
}
