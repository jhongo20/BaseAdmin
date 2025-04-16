using System.Reflection;
using System.Text;
using AspNetCoreRateLimit;
using AuthSystem.API.Extensions;
using AuthSystem.API.Middleware;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Infrastructure.Persistence;
using AuthSystem.Infrastructure.Persistence.Repositories;
using AuthSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

// Configurar Serilog desde appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Iniciando la aplicación AuthSystem");
    
    // Crear el builder de la aplicación
    var builder = WebApplication.CreateBuilder(args);

    // Configurar servicios de rate limiting
    builder.Services.AddMemoryCache();
    builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("RateLimiting"));
    builder.Services.Configure<ClientRateLimitOptions>(builder.Configuration.GetSection("RateLimiting"));
    builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
    builder.Services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
    builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
    builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
    builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
    builder.Services.AddInMemoryRateLimiting();

    // Add services to the container.
    builder.Services.AddControllers();

    // Configurar compresión de respuestas HTTP
    builder.Services.AddResponseCompression(options =>
    {
        options.EnableForHttps = true; // Habilitar compresión también para HTTPS
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
        options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
        options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
            new[] { "application/json", "application/xml", "text/plain", "text/css", "application/javascript", "text/html" });
    });

    // Configurar proveedores de compresión
    builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options =>
    {
        options.Level = System.IO.Compression.CompressionLevel.Optimal;
    });

    builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
    {
        options.Level = System.IO.Compression.CompressionLevel.Optimal;
    });

    // Configurar API versionada
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    });

    builder.Services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // Registrar servicios usando las extensiones
    builder.Services.AddPersistence(builder.Configuration);
    builder.Services.AddRepositories();
    builder.Services.AddApplicationServices();
    builder.Services.AddCacheServices(builder.Configuration);

    // Configuración de servicios de cola de mensajes
    builder.Services.AddMessageQueueServices(builder.Configuration);

    // Registrar servicios de la aplicación
    builder.Services.AddScoped<ITokenRevocationService, TokenRevocationService>();
    builder.Services.AddScoped<ISessionManagementService, SessionManagementService>();
    builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();

    // Configurar autenticación JWT
    builder.Services.AddAuthentication(options =>
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
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"])),
            ClockSkew = TimeSpan.Zero
        };
    });

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();

    // Configurar Swagger para API versionada
    builder.Services.AddSwaggerGen(options =>
    {
        // Obtener todas las versiones de API disponibles
        var provider = builder.Services.BuildServiceProvider().GetRequiredService<Microsoft.AspNetCore.Mvc.ApiExplorer.IApiVersionDescriptionProvider>();
        
        // Crear un documento Swagger para cada versión
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = $"Auth System API {description.GroupName}",
                    Version = description.ApiVersion.ToString(),
                    Description = description.IsDeprecated ? "Esta versión de la API está obsoleta." : "API de autenticación y autorización.",
                    Contact = new OpenApiContact
                    {
                        Name = "Equipo de Desarrollo",
                        Email = "dev@example.com"
                    }
                });
        }
        
        // Configuración de seguridad para Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // Configurar Serilog como el proveedor de logging
    builder.Host.UseSerilog();

    var app = builder.Build();

    // Usar compresión de respuestas HTTP
    app.UseResponseCompression();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetService<Microsoft.AspNetCore.Mvc.ApiExplorer.IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
        });
    }

    // Configurar middleware de rate limiting
    app.UseIpRateLimiting();
    app.UseClientRateLimiting();
    app.UseRateLimitingLogger();

    // Agregar middleware de logging
    app.UseLogging();

    // Agregar middleware de seguridad
    app.UseSecurityHeaders();

    // Agregar middleware de auditoría
    app.UseAuditMiddleware();

    app.UseHttpsRedirection();
    app.UseSecurityHeaders();
    app.UseTokenRevocation();
    app.UseSessionValidation();
    app.UseFeatureFlags();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Aplicar migraciones automáticamente al iniciar la aplicación
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }

    // Crear el directorio de logs si no existe
    var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
    if (!Directory.Exists(logDirectory))
    {
        Directory.CreateDirectory(logDirectory);
    }

    Log.Information("Aplicación AuthSystem iniciada correctamente");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación AuthSystem se detuvo inesperadamente");
}
finally
{
    Log.CloseAndFlush();
}
