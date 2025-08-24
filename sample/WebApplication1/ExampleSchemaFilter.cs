using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebApplication1.Domain.Entities;

namespace WebApplication1
{
    public class ExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(Author))
            {
                schema.Example = new OpenApiObject()
                {
                    ["firstName"] = new OpenApiString("John"),
                    ["lastName"] = new OpenApiString("Doe"),
                };
            }


            if (context.Type == typeof(Book))
            {
                schema.Example = new OpenApiObject()
                {
                    ["id"] = new OpenApiString("9780323776714"),
                    ["authorId"] = new OpenApiInteger(1),
                    ["title"] = new OpenApiString("The best book"),
                };
            }
        }
    }
}
