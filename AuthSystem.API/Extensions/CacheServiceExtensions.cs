using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models;
using AuthSystem.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AuthSystem.API.Extensions
{
    /// <summary>
    /// Extensiones para registrar servicios de caché
    /// </summary>
    public static class CacheServiceExtensions
    {
        /// <summary>
        /// Añade servicios de caché a la colección de servicios
        /// </summary>
        /// <param name="services">Colección de servicios</param>
        /// <param name="configuration">Configuración</param>
        /// <returns>Colección de servicios</returns>
        public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar la configuración de caché
            var cacheSettings = configuration.GetSection("CacheSettings").Get<CacheSettings>() ?? new CacheSettings();
            services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));

            // Registrar servicio de métricas de caché
            services.AddSingleton<ICacheMetricsService, CacheMetricsService>();
            
            // Registrar servicio de invalidación de caché
            services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();

            // Registrar caché en memoria
            services.AddMemoryCache();

            // Configurar caché distribuida según el proveedor
            if (string.Equals(cacheSettings.Provider, "Redis", StringComparison.OrdinalIgnoreCase))
            {
                // Configurar Redis
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = configuration["Redis:ConnectionString"];
                    options.InstanceName = configuration["Redis:InstanceName"];
                });

                // Registrar servicio de caché con Redis
                services.AddScoped<ICacheService, RedisCacheService>();
            }
            else
            {
                // Registrar servicio de caché con memoria
                services.AddScoped<ICacheService, MemoryCacheService>();
            }

            return services;
        }
    }
}
