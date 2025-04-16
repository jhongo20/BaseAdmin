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

## Métricas de Caché

El sistema de caché incluye un servicio de métricas que permite monitorear el rendimiento y uso de la caché. Esto facilita la identificación de problemas y la optimización de la configuración.

### Métricas disponibles

- **Tasa de aciertos (Hit Rate)**: Porcentaje de solicitudes que encontraron el valor en caché.
- **Tiempo de respuesta**: Tiempo promedio para recuperar valores de la caché.
- **Número de elementos**: Cantidad de elementos actualmente en caché.
- **Expiraciones**: Número de elementos que han expirado automáticamente.
- **Eliminaciones manuales**: Número de elementos eliminados manualmente.
- **Métricas por tipo**: Estadísticas desglosadas por tipo de clave (LDAP, configuración, usuarios, etc.).

### Acceso a las métricas

Las métricas de caché están disponibles a través del endpoint `/api/CacheMetrics`, que requiere autenticación con rol de administrador.

### Ejemplo de uso

```csharp
// Inyectar el servicio de métricas
private readonly ICacheMetricsService _metricsService;

public MyService(ICacheMetricsService metricsService)
{
    _metricsService = metricsService;
}

// Obtener un resumen de las métricas
public Dictionary<string, object> GetCacheMetricsSummary()
{
    return _metricsService.GetMetricsSummary();
}

// Reiniciar las métricas
public void ResetCacheMetrics()
{
    _metricsService.ResetMetrics();
}

// Exportar las métricas a un archivo JSON
public async Task<bool> ExportCacheMetricsAsync(string filePath)
{
    return await _metricsService.ExportMetricsToJsonAsync(filePath);
}
```

## Invalidación Selectiva de Caché

El sistema de caché incluye un mecanismo de invalidación selectiva que permite eliminar entradas específicas de la caché cuando los datos cambian en la fuente.

### Tipos de invalidación

- **Invalidación por usuario**: Elimina todas las entradas de caché relacionadas con un usuario específico.
- **Invalidación por organización**: Elimina todas las entradas de caché relacionadas con una organización específica.
- **Invalidación por rol**: Elimina todas las entradas de caché relacionadas con un rol específico.
- **Invalidación por permiso**: Elimina todas las entradas de caché relacionadas con un permiso específico.
- **Invalidación de configuración LDAP**: Elimina la configuración LDAP de una organización específica.
- **Invalidación de información de usuario LDAP**: Elimina la información de un usuario LDAP específico.
- **Invalidación completa de LDAP**: Elimina todas las entradas de caché relacionadas con LDAP.
- **Invalidación completa de configuración**: Elimina todas las entradas de caché relacionadas con configuración.

### Acceso a la invalidación

La invalidación de caché está disponible a través del endpoint `/api/cache/invalidate`, que requiere autenticación con rol de administrador.

### Ejemplo de uso

```csharp
// Inyectar el servicio de invalidación
private readonly ICacheInvalidationService _cacheInvalidationService;

public MyService(ICacheInvalidationService cacheInvalidationService)
{
    _cacheInvalidationService = cacheInvalidationService;
}

// Invalidar caché de usuario
public async Task<int> InvalidateUserCacheAsync(string userId)
{
    return await _cacheInvalidationService.InvalidateUserCacheAsync(userId);
}

// Invalidar configuración LDAP
public async Task<int> InvalidateLdapConfigCacheAsync(string organizationId)
{
    return await _cacheInvalidationService.InvalidateLdapConfigCacheAsync(organizationId);
}

// Invalidar toda la caché de LDAP
public async Task<int> InvalidateAllLdapCacheAsync()
{
    return await _cacheInvalidationService.InvalidateAllLdapCacheAsync();
}
```

## Compresión de Datos en Caché

El sistema de caché incluye un mecanismo de compresión que reduce el tamaño de los datos almacenados en caché, lo que permite almacenar más información en la misma cantidad de memoria.

### Funcionamiento

- Los datos se comprimen automáticamente si superan un umbral de tamaño configurable (por defecto, 1KB).
- La compresión utiliza el algoritmo GZip, que proporciona una buena relación entre velocidad y tasa de compresión.
- Los datos comprimidos se identifican con un prefijo especial, lo que permite descomprimirlos automáticamente al recuperarlos.

### Configuración

La compresión se configura en el archivo `appsettings.json`:

```json
"CacheSettings": {
  "EnableCompression": true,
  "CompressionThresholdBytes": 1024
}
```

- **EnableCompression**: Habilita o deshabilita la compresión de datos.
- **CompressionThresholdBytes**: Umbral de tamaño en bytes a partir del cual se comprimen los datos.

### Ejemplo de uso

La compresión es transparente para el usuario del sistema de caché. Los datos se comprimen y descomprimen automáticamente según sea necesario.

```csharp
// Los datos grandes se comprimen automáticamente
await _cacheService.SetAsync("key", largeObject);

// Los datos se descomprimen automáticamente al recuperarlos
var largeObject = await _cacheService.GetAsync<LargeObject>("key");
```

## Extensión a Otros Servicios

El sistema de caché está diseñado para ser fácilmente extensible a otros servicios de la aplicación. A continuación, se muestra cómo aplicar caché a otros servicios:

### Ejemplo: Caché para servicio de usuarios

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICacheService _cacheService;
    private readonly CacheSettings _cacheSettings;

    public UserService(
        IUserRepository userRepository,
        ICacheService cacheService,
        IOptions<CacheSettings> cacheSettings)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<User> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Definir clave de caché
        string cacheKey = $"user:{userId}";
        
        // Intentar obtener de la caché
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _userRepository.GetByIdAsync(userId, cancellationToken),
            _cacheSettings.UserCacheAbsoluteExpiration,
            null,
            cancellationToken);
    }

    public async Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        // Actualizar en la base de datos
        bool result = await _userRepository.UpdateAsync(user, cancellationToken);
        
        if (result)
        {
            // Invalidar caché
            string cacheKey = $"user:{user.Id}";
            await _cacheService.RemoveAsync(cacheKey, cancellationToken);
        }
        
        return result;
    }
}
```

### Ejemplo: Caché para servicio de roles

```csharp
public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly ICacheService _cacheService;
    private readonly CacheSettings _cacheSettings;

    public RoleService(
        IRoleRepository roleRepository,
        ICacheService cacheService,
        IOptions<CacheSettings> cacheSettings)
    {
        _roleRepository = roleRepository;
        _cacheService = cacheService;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<Role> GetRoleByIdAsync(string roleId, CancellationToken cancellationToken = default)
    {
        // Definir clave de caché
        string cacheKey = $"role:{roleId}";
        
        // Intentar obtener de la caché
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _roleRepository.GetByIdAsync(roleId, cancellationToken),
            _cacheSettings.RoleCacheAbsoluteExpiration,
            null,
            cancellationToken);
    }

    public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        // Definir clave de caché
        string cacheKey = $"user:{userId}:roles";
        
        // Intentar obtener de la caché
        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _roleRepository.GetByUserIdAsync(userId, cancellationToken),
            _cacheSettings.RoleCacheAbsoluteExpiration,
            null,
            cancellationToken);
    }
}
```

## Conclusión

El sistema de caché implementado proporciona una solución robusta y flexible para almacenar resultados de consultas frecuentes, mejorando significativamente el rendimiento de la aplicación. La abstracción a través de la interfaz `ICacheService` permite cambiar fácilmente entre diferentes proveedores de caché según las necesidades del entorno.

Con las mejoras implementadas (métricas, invalidación selectiva, compresión y extensión a otros servicios), el sistema de caché es ahora más eficiente, más fácil de monitorear y más flexible para adaptarse a diferentes escenarios de uso.
