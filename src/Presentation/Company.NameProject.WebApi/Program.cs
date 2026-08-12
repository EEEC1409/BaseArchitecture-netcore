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
using Serilog.Context;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;

using System.Collections.Generic;
using System.Text;

static bool IsPostgreSqlConnectionString(string? connectionString)
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return false;
    }

    return connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase)
        && connectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase)
        && connectionString.Contains("Username=", StringComparison.OrdinalIgnoreCase);
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, services, config) =>
    {
        var connectionString = ctx.Configuration.GetConnectionString("DefaultConnection");
        var serilogPostgreSqlConnectionString =
            ctx.Configuration["Serilog:PostgreSql:ConnectionString"]
            ?? ctx.Configuration.GetConnectionString("SerilogPostgreSql");

        if (!IsPostgreSqlConnectionString(serilogPostgreSqlConnectionString)
            && IsPostgreSqlConnectionString(connectionString))
        {
            serilogPostgreSqlConnectionString = connectionString;
        }

        var logTableName = ctx.Configuration["Serilog:PostgreSql:TableName"] ?? "application_logs";

        config.ReadFrom.Configuration(ctx.Configuration)
              .ReadFrom.Services(services)
              // Fallback para contenedores: garantiza salida a stdout/stderr
              .WriteTo.Console();

        if (IsPostgreSqlConnectionString(serilogPostgreSqlConnectionString))
        {
            var columnWriters = new Dictionary<string, ColumnWriterBase>
            {
                ["message"] = new RenderedMessageColumnWriter(),
                ["message_template"] = new MessageTemplateColumnWriter(),
                ["level"] = new LevelColumnWriter(),
                ["raise_date"] = new TimestampColumnWriter(),
                ["exception"] = new ExceptionColumnWriter(),
                ["properties"] = new LogEventSerializedColumnWriter(),
                ["token"] = new SinglePropertyColumnWriter("Token"),
                ["tipo_transaccion"] = new SinglePropertyColumnWriter("TipoTransaccion"),
                ["metodo"] = new SinglePropertyColumnWriter("Metodo"),
                ["capa"] = new SinglePropertyColumnWriter("Capa")
            };

            config.WriteTo.PostgreSQL(
                connectionString: serilogPostgreSqlConnectionString!,
                tableName: logTableName,
                columnOptions: columnWriters,
                needAutoCreateTable: false,
                restrictedToMinimumLevel: LogEventLevel.Warning);
        }
    });

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

    app.Use(async (context, next) =>
    {
        var token = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? context.TraceIdentifier;

        var metodo = $"{context.Request.Method} {context.Request.Path}";
        const string capa = "Presentation";

        context.Response.Headers["X-Correlation-ID"] = token;

        using (LogContext.PushProperty("CorrelationId", token))
        using (LogContext.PushProperty("Token", token))
        using (LogContext.PushProperty("Metodo", metodo))
        using (LogContext.PushProperty("Capa", capa))
        {
            await next();
        }
    });

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "Transaccion HTTP procesada";
        options.GetLevel = (httpContext, _, exception) =>
        {
            if (exception is not null || httpContext.Response.StatusCode >= 500)
            {
                return LogEventLevel.Error;
            }

            if (httpContext.Response.StatusCode >= 400)
            {
                return LogEventLevel.Warning;
            }

            return LogEventLevel.Information;
        };

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            var token = httpContext.Response.Headers["X-Correlation-ID"].ToString();
            var metodo = $"{httpContext.Request.Method} {httpContext.Request.Path}";
            var tipoTransaccion = httpContext.Response.StatusCode >= 500
                ? "ERROR"
                : httpContext.Response.StatusCode >= 400
                    ? "WAR"
                    : "OK";

            diagnosticContext.Set("CorrelationId", token);
            diagnosticContext.Set("Token", token);
            diagnosticContext.Set("Metodo", metodo);
            diagnosticContext.Set("Capa", "Presentation");
            diagnosticContext.Set("TipoTransaccion", tipoTransaccion);
        };
    });

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


