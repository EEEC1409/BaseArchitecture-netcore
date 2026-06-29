using Company.NameProject.WebApi.Auth;
using Company.NameProject.WebApi.Middleware;
using Company.NameProject.WebApi.Options;
using Company.NameProject.WebApi.Swagger;
using Company.NameProject.Application;
using Company.NameProject.Infrastructure;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
#if (IncludeEureka)
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Options;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Eureka;
#endif

using Serilog;

using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, services, config) =>
        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              // Fallback para contenedores: garantiza salida a stdout/stderr
              .WriteTo.Console());

    // Layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

#if (IncludeEureka)
    // Eureka — instanceId dinámico por variable de entorno
    var instanceId = Environment.GetEnvironmentVariable("INSTANCE_ID")
        ?? $"NameProject-service:{Guid.NewGuid()}";
    builder.Configuration["eureka:instance:instanceId"] = instanceId;
    builder.Services.AddDiscoveryClient(builder.Configuration);
#endif

    // JWT
    var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

    builder.Services.AddAuthorization();

    // CORS
    var allowedOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                // En desarrollo se permite cualquier origen para facilitar el trabajo local
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
                return;
            }

            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    // ASP.NET
    builder.Services.AddControllers();

    // Swagger con JWT y documentación XML
    builder.Services.AddSwaggerWithJwt(builder.Configuration);

    var app = builder.Build();

#if (IncludeEureka)
    // Registrar el puerto dinámico en Eureka al arrancar el host
    var eurekaOptions = app.Services.GetRequiredService<IOptions<EurekaInstanceOptions>>().Value;
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        var addressesFeature = app.Services
            .GetRequiredService<IServer>()
            .Features
            .Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();

        var firstAddress = addressesFeature?.Addresses.FirstOrDefault();
        if (firstAddress != null)
        {
            var dynamicPort = new Uri(firstAddress).Port;
            eurekaOptions.Port = dynamicPort;
            eurekaOptions.NonSecurePort = dynamicPort;
            Log.Information("Puerto registrado en Eureka: {Port}", dynamicPort);
        }
    });
#endif

    // Middleware pipeline (order matters)
    // Reconstruye Host/Scheme/PathBase desde headers X-Forwarded-* del proxy inverso (Nginx/Traefik/Swarm)
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor
                         | ForwardedHeaders.XForwardedProto
                         | ForwardedHeaders.XForwardedHost
                         | ForwardedHeaders.XForwardedPrefix
    });

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSerilogRequestLogging();

    // Swagger (todos los entornos — ajustar según política)
    app.UseSwaggerWithJwt();

    // HTTPS solo fuera de desarrollo (en contenedores el proxy externo maneja TLS)
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    app.UseCors();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Iniciando Company.NameProject.WebApi...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}


