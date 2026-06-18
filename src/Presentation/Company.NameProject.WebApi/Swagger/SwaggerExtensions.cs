using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;

namespace Company.NameProject.WebApi.Swagger;

/// <summary>
/// Extensiones de registro y configuración de Swagger/OpenAPI para la capa de presentación.
/// Encapsula: metadatos de la API, esquema de seguridad JWT Bearer y filtros de documentación.
/// </summary>
public static class SwaggerExtensions
{
    private const string BearerScheme = "Bearer";
    private const string SecurityDefinitionId = "Bearer";

    /// <summary>
    /// Registra Swagger con soporte completo para JWT Bearer y documentación XML.
    /// </summary>
    public static IServiceCollection AddSwaggerWithJwt(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiInfo = configuration.GetSection("ApiInfo");

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            // ─── Metadatos de la API ────────────────────────────────────────
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title       = apiInfo["Title"]       ?? "Company.NameProject API",
                Version     = apiInfo["Version"]     ?? "v1",
                Description = apiInfo["Description"] ?? "API generada con la plantilla de Clean Architecture.",
                Contact = new OpenApiContact
                {
                    Name  = apiInfo["ContactName"]  ?? "Equipo de Desarrollo",
                    Email = apiInfo["ContactEmail"] ?? string.Empty
                }
            });

            // ─── Esquema de seguridad JWT Bearer ───────────────────────────
            options.AddSecurityDefinition(SecurityDefinitionId, new OpenApiSecurityScheme
            {
                Name         = "Authorization",
                Type         = SecuritySchemeType.Http,
                Scheme       = BearerScheme,
                BearerFormat = "JWT",
                In           = ParameterLocation.Header,
                Description  =
                    "Ingresa el token JWT.\n\n" +
                    "Ejemplo: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = SecurityDefinitionId
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // ─── Documentación XML (comments de ///  en controllers) ───────
            var xmlFiles = Directory
                .GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);

            foreach (var xmlFile in xmlFiles)
                options.IncludeXmlComments(xmlFile, includeControllerXmlComments: true);

            // ─── Ordena los endpoints por ruta ─────────────────────────────
            options.OrderActionsBy(api => api.RelativePath);
        });

        return services;
    }

    /// <summary>
    /// Agrega el middleware de Swagger UI con el botón "Authorize" visible.
    /// Disponible en todos los entornos; ajusta la condición según política de tu equipo.
    /// </summary>
    public static WebApplication UseSwaggerWithJwt(this WebApplication app)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "swagger/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            options.RoutePrefix        = "swagger";
            options.DocumentTitle      = "API Docs";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();

            // Muestra el esquema de seguridad desplegado por defecto
            options.DefaultModelsExpandDepth(-1);
        });

        return app;
    }
}
