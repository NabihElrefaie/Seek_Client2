using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Seek.API.Services.System
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        readonly IApiVersionDescriptionProvider provider;
        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => this.provider = provider;
        public void Configure(SwaggerGenOptions options)
        {
            foreach (var desc in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc
                    (
                    desc.GroupName, new OpenApiInfo()
                    {
                        Title = $"Seek {desc.ApiVersion}",
                        Version = desc.ApiVersion.ToString(),
                        Description = "A Middleware for ZATCA E-Invoicing Integration",
                        Contact = new OpenApiContact()
                        {
                            Email = "Nabihabdelkhalek6@gmail.com",
                            Name = "Nabih",
                            Url = new Uri("https://fb.com/nabihabdelkhalek")
                        },
                        License = new OpenApiLicense()
                        {
                            Name = "IIS",
                            Url = new Uri("https://mystation.sa")
                        }
                    }
                    );
            }
            //Add xml Comments to Swagger
            var xmlCommentFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var cmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentFile);
            options.IncludeXmlComments(cmlCommentsFullPath);
        }

    }
}
