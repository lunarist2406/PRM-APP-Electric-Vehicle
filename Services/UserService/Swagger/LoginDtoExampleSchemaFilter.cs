using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using UserService.Models.DTOs;

namespace UserService.Swagger
{
    public class LoginDtoExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(LoginDto))
            {
                schema.Example = new OpenApiObject
                {
                    ["email"] = new OpenApiString("nguyenvana@gmail.com"),
                    ["password"] = new OpenApiString("123456")
                };
            }
        }
    }
}
