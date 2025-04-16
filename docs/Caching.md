# Sistema de Caché

## Descripción

El sistema de caché implementado permite almacenar resultados de consultas frecuentes, reduciendo la carga en servicios externos y bases de datos, y mejorando significativamente el rendimiento de la aplicación. Está especialmente diseñado para almacenar resultados de consultas LDAP y configuración, que son operaciones costosas y que no cambian con frecuencia.

## Arquitectura

El sistema de caché está compuesto por:

1. **Interfaz `ICacheService`**: Define las operaciones comunes para interactuar con la caché.
2. **Implementación `RedisCacheService`**: Utiliza Redis como proveedor de caché distribuida.
3. **Implementación `MemoryCacheService`**: Utiliza la memoria del servidor como alternativa para entornos donde Redis no está disponible.
4. **Configuración `CacheSettings`**: Define los tiempos de expiración y otras configuraciones para diferentes tipos de datos.
5. **Extensiones `CacheServiceExtensions`**: Facilita el registro de los servicios de caché en el contenedor de dependencias.

## Configuración

La configuración del sistema de caché se realiza en el archivo `appsettings.json`:

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "AuthSystem:"
  },
  "CacheSettings": {
    "Provider": "Redis", 
    "DefaultAbsoluteExpirationMinutes": 60,
    "DefaultSlidingExpirationMinutes": 20,
    "LdapCacheAbsoluteExpirationMinutes": 120,
    "ConfigurationCacheAbsoluteExpirationMinutes": 240,
    "UserCacheAbsoluteExpirationMinutes": 30,
    "RoleCacheAbsoluteExpirationMinutes": 60,
    "PermissionCacheAbsoluteExpirationMinutes": 60
  }
}
```

### Explicación de la configuración

- **Provider**: Proveedor de caché a utilizar (`Redis` o `Memory`).
- **DefaultAbsoluteExpirationMinutes**: Tiempo de expiración absoluto predeterminado en minutos.
- **DefaultSlidingExpirationMinutes**: Tiempo de expiración deslizante predeterminado en minutos.
- **LdapCacheAbsoluteExpirationMinutes**: Tiempo de expiración para la caché de LDAP.
- **ConfigurationCacheAbsoluteExpirationMinutes**: Tiempo de expiración para la caché de configuración.
- **UserCacheAbsoluteExpirationMinutes**: Tiempo de expiración para la caché de usuarios.
- **RoleCacheAbsoluteExpirationMinutes**: Tiempo de expiración para la caché de roles.
- **PermissionCacheAbsoluteExpirationMinutes**: Tiempo de expiración para la caché de permisos.

## Implementación

### Interfaz ICacheService

La interfaz `ICacheService` define las operaciones comunes para interactuar con la caché:

```csharp
public interface ICacheService
{
    Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);
    Task<long> IncrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);
    Task<long> DecrementAsync(string key, long value = 1, CancellationToken cancellationToken = default);
    Task<bool> RefreshExpirationAsync(string key, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);
    Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task<bool> ClearAsync(CancellationToken cancellationToken = default);
    Task<long> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default);
}
```

### Implementación RedisCacheService

La implementación `RedisCacheService` utiliza Redis como proveedor de caché distribuida:

```csharp
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    // Implementación de los métodos de ICacheService
}
```

### Implementación MemoryCacheService

La implementación `MemoryCacheService` utiliza la memoria del servidor como alternativa para entornos donde Redis no está disponible:

```csharp
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;
    private readonly ConcurrentDictionary<string, bool> _keys = new ConcurrentDictionary<string, bool>();

    public MemoryCacheService(IMemoryCache memoryCache, ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    // Implementación de los métodos de ICacheService
}
```

### Registro de servicios

El registro de los servicios de caché se realiza en el método `AddCacheServices` de la clase `CacheServiceExtensions`:

```csharp
public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
{
    // Registrar la configuración de caché
    var cacheSettings = configuration.GetSection("CacheSettings").Get<CacheSettings>() ?? new CacheSettings();
    services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));

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
```

## Uso

### Inyección de dependencias

Para utilizar el servicio de caché, se debe inyectar la interfaz `ICacheService` en el constructor de la clase:

```csharp
public class MyService
{
    private readonly ICacheService _cacheService;
    private readonly CacheSettings _cacheSettings;

    public MyService(ICacheService cacheService, IOptions<CacheSettings> cacheSettings)
    {
        _cacheService = cacheService;
        _cacheSettings = cacheSettings.Value;
    }

    // Métodos de la clase
}
```

### Almacenar un valor en la caché

```csharp
public async Task<bool> StoreValueAsync<T>(string key, T value, CancellationToken cancellationToken = default)
{
    TimeSpan expiration = TimeSpan.FromMinutes(_cacheSettings.DefaultAbsoluteExpirationMinutes);
    return await _cacheService.SetAsync(key, value, expiration, null, cancellationToken);
}
```

### Obtener un valor de la caché

```csharp
public async Task<T> GetValueAsync<T>(string key, CancellationToken cancellationToken = default)
{
    return await _cacheService.GetAsync<T>(key, cancellationToken);
}
```

### Obtener o establecer un valor en la caché

```csharp
public async Task<T> GetOrSetValueAsync<T>(string key, Func<Task<T>> factory, CancellationToken cancellationToken = default)
{
    TimeSpan expiration = TimeSpan.FromMinutes(_cacheSettings.DefaultAbsoluteExpirationMinutes);
    return await _cacheService.GetOrSetAsync(key, factory, expiration, null, cancellationToken);
}
```

### Eliminar un valor de la caché

```csharp
public async Task<bool> RemoveValueAsync(string key, CancellationToken cancellationToken = default)
{
    return await _cacheService.RemoveAsync(key, cancellationToken);
}
```

### Eliminar valores por patrón

```csharp
public async Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
{
    return await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
}
```

### Limpiar toda la caché

```csharp
public async Task<bool> ClearCacheAsync(CancellationToken cancellationToken = default)
{
    return await _cacheService.ClearAsync(cancellationToken);
}
```

### Obtener tiempo de expiración

```csharp
public async Task<long> GetTimeToLiveAsync(string key, CancellationToken cancellationToken = default)
{
    return await _cacheService.GetTimeToLiveAsync(key, cancellationToken);
}
```

### Incrementar un contador en la caché

```csharp
public async Task<long> IncrementCounterAsync(string key, long value = 1, CancellationToken cancellationToken = default)
{
    return await _cacheService.IncrementAsync(key, value, cancellationToken);
}
```

## Consideraciones de rendimiento

- **Tamaño de la caché**: Monitorear el uso de memoria de la caché para evitar problemas de rendimiento.
- **Tiempo de expiración**: Configurar tiempos de expiración adecuados para cada tipo de dato.
- **Serialización**: La serialización y deserialización de objetos complejos puede afectar el rendimiento.
- **Distribución**: En entornos distribuidos, utilizar Redis para compartir la caché entre múltiples instancias.
- **Invalidación**: Implementar mecanismos de invalidación de caché cuando los datos cambien.

## Monitoreo y diagnóstico

- **Logs**: El sistema de caché registra eventos de depuración, información y error.
- **Métricas**: Monitorear el rendimiento de la caché (tasa de aciertos, tasa de fallos, tiempo de respuesta).
- **Herramientas**: Utilizar herramientas como Redis Desktop Manager para inspeccionar la caché.

## Extensibilidad

Para añadir soporte para otros proveedores de caché:

1. Implementar la interfaz `ICacheService` para el nuevo proveedor.
2. Actualizar la clase `CacheServiceExtensions` para registrar el nuevo proveedor.
3. Añadir la opción correspondiente en la configuración.

## Conclusión

El sistema de caché implementado proporciona una solución robusta y flexible para almacenar resultados de consultas frecuentes, mejorando significativamente el rendimiento de la aplicación. La abstracción a través de la interfaz `ICacheService` permite cambiar fácilmente entre diferentes proveedores de caché según las necesidades del entorno.

## Ejemplo práctico: Caché en el servicio LDAP

El servicio LDAP utiliza el sistema de caché para almacenar la configuración LDAP y la información de usuarios:

### Caché de configuración LDAP

```csharp
private async Task<LdapConfig> GetLdapConfigAsync(Guid organizationId, CancellationToken cancellationToken)
{
    // Intentar obtener de la caché primero
    string cacheKey = $"ldap:config:{organizationId}";
    var cachedConfig = await _cacheService.GetAsync<LdapConfig>(cacheKey, cancellationToken);
    if (cachedConfig != null)
    {
        _logger.LogDebug("LDAP configuration retrieved from cache for organization {OrganizationId}", organizationId);
        return cachedConfig;
    }

    // Obtener la configuración de la fuente
    var config = new LdapConfig
    {
        Server = _configuration["LdapSettings:Server"],
        Port = int.Parse(_configuration["LdapSettings:Port"]),
        BindDn = _configuration["LdapSettings:BindDN"],
        BindPassword = _configuration["LdapSettings:BindPassword"],
        SearchBase = _configuration["LdapSettings:SearchBase"],
        SearchFilter = _configuration["LdapSettings:SearchFilter"],
        UserDnFormat = "uid={0}," + _configuration["LdapSettings:SearchBase"]
    };

    // Guardar en caché
    TimeSpan cacheExpiration = TimeSpan.FromMinutes(_cacheSettings.ConfigurationCacheAbsoluteExpirationMinutes);
    await _cacheService.SetAsync(cacheKey, config, cacheExpiration, null, cancellationToken);
    _logger.LogDebug("LDAP configuration stored in cache for organization {OrganizationId}", organizationId);

    return config;
}
```

### Caché de información de usuarios LDAP

```csharp
public async Task<LdapUserInfo> GetUserInfoAsync(string username, Guid organizationId, CancellationToken cancellationToken = default)
{
    try
    {
        // Intentar obtener de la caché primero
        string cacheKey = $"ldap:userinfo:{organizationId}:{username}";
        var cachedUserInfo = await _cacheService.GetAsync<LdapUserInfo>(cacheKey, cancellationToken);
        if (cachedUserInfo != null)
        {
            _logger.LogDebug("LDAP user info retrieved from cache for {Username}", username);
            return cachedUserInfo;
        }

        // Obtener la información del usuario de LDAP
        // ...

        // Guardar en caché
        TimeSpan cacheExpiration = TimeSpan.FromMinutes(_cacheSettings.LdapCacheAbsoluteExpirationMinutes);
        await _cacheService.SetAsync(cacheKey, userInfo, cacheExpiration, null, cancellationToken);
        _logger.LogDebug("LDAP user info stored in cache for {Username}", username);

        return userInfo;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting LDAP user info for {Username}", username);
        return null;
    }
}
