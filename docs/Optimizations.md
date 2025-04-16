# Optimizaciones de Rendimiento

Este documento describe las optimizaciones de rendimiento implementadas en el sistema de autenticación para mejorar la velocidad, reducir el consumo de recursos y proporcionar una mejor experiencia de usuario.

## Tabla de Contenidos

1. [Compresión de Respuestas HTTP](#compresión-de-respuestas-http)
2. [Optimización de Consultas SQL](#optimización-de-consultas-sql)
3. [Extensiones de Optimización de Consultas](#extensiones-de-optimización-de-consultas)
4. [Mejores Prácticas](#mejores-prácticas)

## Compresión de Respuestas HTTP

La compresión de respuestas HTTP reduce significativamente el tamaño de los datos transferidos entre el servidor y el cliente, lo que resulta en tiempos de carga más rápidos y menor consumo de ancho de banda.

### Tecnologías Implementadas

- **Brotli**: Algoritmo de compresión moderno y eficiente desarrollado por Google.
- **Gzip**: Algoritmo de compresión ampliamente compatible con navegadores más antiguos.

### Configuración

La compresión de respuestas HTTP se configura en el archivo `Program.cs`:

```csharp
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
```

### Activación del Middleware

El middleware de compresión se activa en la canalización de solicitudes HTTP:

```csharp
// Usar compresión de respuestas HTTP
app.UseResponseCompression();
```

### Beneficios

- **Reducción del tamaño de respuesta**: Hasta un 70-80% para respuestas JSON y HTML.
- **Menor tiempo de carga**: Especialmente notable en conexiones de red lentas o con alta latencia.
- **Reducción del consumo de ancho de banda**: Menor costo de transferencia de datos.
- **Mejor experiencia de usuario**: Tiempos de respuesta más rápidos.

### Tipos MIME Comprimidos

La compresión se aplica a los siguientes tipos MIME:

- `application/json`
- `application/xml`
- `text/plain`
- `text/css`
- `application/javascript`
- `text/html`
- Otros tipos predeterminados de ASP.NET Core

## Optimización de Consultas SQL

La optimización de consultas SQL mejora el rendimiento de la base de datos mediante la creación de índices adecuados y la estructuración eficiente de las consultas.

### Índices Implementados

Se han añadido índices a las tablas más consultadas para mejorar el rendimiento:

#### Tabla de Usuarios (`Users`)
- `IX_Users_LastLoginDate`: Optimiza consultas que filtran por fecha de último inicio de sesión.
- `IX_Users_Status`: Mejora el rendimiento de consultas que filtran por estado de usuario.
- `IX_Users_CreatedAt`: Optimiza consultas que ordenan o filtran por fecha de creación.

#### Tabla de Sesiones de Usuario (`UserSessions`)
- `IX_UserSessions_CreatedAt`: Mejora consultas que ordenan por fecha de creación.
- `IX_UserSessions_ExpiresAt`: Optimiza la limpieza de sesiones expiradas.
- `IX_UserSessions_Status`: Mejora consultas que filtran por estado de sesión.

#### Tabla de Intentos de Inicio de Sesión (`UserLoginAttempts`)
- `IX_UserLoginAttempts_AttemptDate`: Optimiza consultas de monitoreo de seguridad.
- `IX_UserLoginAttempts_IpAddress`: Mejora la detección de intentos de fuerza bruta.
- `IX_UserLoginAttempts_Username`: Optimiza la detección de ataques dirigidos.

#### Tabla de Logs de Auditoría (`AuditLogs`)
- `IX_AuditLogs_EventDate`: Mejora consultas que filtran por fecha de evento.
- `IX_AuditLogs_EventType`: Optimiza consultas que filtran por tipo de evento.
- `IX_AuditLogs_EntityName_EntityId`: Mejora búsquedas de auditoría por entidad.

#### Otras Tablas
- Índices para relaciones (`UserRoles`, `RolePermissions`, `UserBranches`).
- Índices para tablas de referencia (`Permissions`, `Branches`).
- Índices para gestión de tokens (`RevokedTokens`).

### Implementación

Los índices se implementan a través de una migración de Entity Framework Core:

```csharp
public partial class OptimizationIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Creación de índices...
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Eliminación de índices...
    }
}
```

### Beneficios

- **Consultas más rápidas**: Reducción significativa en tiempos de respuesta.
- **Menor carga del servidor**: Uso más eficiente de recursos de CPU y memoria.
- **Mejor escalabilidad**: Soporte para mayor número de usuarios concurrentes.
- **Optimización de joins**: Mejora en consultas que relacionan múltiples tablas.

## Extensiones de Optimización de Consultas

Se han implementado extensiones para optimizar las consultas de Entity Framework Core:

### Métodos Disponibles

- **OptimizeQuery<T>**: Aplica optimizaciones básicas como `AsNoTracking()`.
- **ApplyPaging<T>**: Implementa paginación eficiente.
- **ApplySorting<T, TKey>**: Aplica ordenamiento optimizado.
- **ApplyFilter<T>**: Aplica filtrado eficiente.
- **ApplyProjection<T, TResult>**: Implementa proyecciones para seleccionar solo los campos necesarios.
- **ApplyIncludes<T>**: Aplica carga anticipada optimizada.
- **ApplyFilteredInclude<T, TProperty>**: Aplica carga anticipada con filtrado.

### Ejemplo de Uso

```csharp
// Consulta optimizada con paginación, filtrado y proyección
var users = _dbContext.Users
    .OptimizeQuery()
    .ApplyFilter(u => u.Status == UserStatus.Active)
    .ApplySorting(u => u.CreatedAt, ascending: false)
    .ApplyIncludes(u => u.UserRoles)
    .ApplyProjection(u => new UserDto
    {
        Id = u.Id,
        Username = u.Username,
        Email = u.Email,
        Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
    })
    .ApplyPaging(page: 1, pageSize: 10);
```

### Beneficios

- **Código más limpio**: Métodos de extensión que encapsulan prácticas recomendadas.
- **Consistencia**: Aplicación uniforme de optimizaciones en toda la aplicación.
- **Mantenibilidad**: Centralización de lógica de optimización.
- **Rendimiento**: Implementación de patrones optimizados para Entity Framework Core.

## Mejores Prácticas

### Compresión HTTP

1. **Monitoreo**: Verificar periódicamente la tasa de compresión y el impacto en el rendimiento.
2. **Caché**: Combinar compresión con políticas de caché adecuadas.
3. **Tamaño mínimo**: Considerar establecer un tamaño mínimo para la compresión (por ejemplo, no comprimir respuestas menores a 1KB).

### Consultas SQL

1. **Análisis de consultas**: Utilizar herramientas como SQL Server Profiler para identificar consultas lentas.
2. **Plan de ejecución**: Revisar planes de ejecución para identificar oportunidades de optimización.
3. **Índices selectivos**: Crear índices solo para columnas frecuentemente consultadas.
4. **Mantenimiento de índices**: Programar reconstrucción y reorganización periódica de índices.

### Entity Framework Core

1. **AsNoTracking**: Usar para consultas de solo lectura.
2. **Carga selectiva**: Cargar solo las propiedades y relaciones necesarias.
3. **Consultas compiladas**: Utilizar para consultas frecuentes.
4. **Transacciones explícitas**: Agrupar operaciones relacionadas en transacciones.

## Conclusión

Las optimizaciones implementadas mejoran significativamente el rendimiento del sistema, reduciendo tiempos de respuesta y consumo de recursos. Estas mejoras son especialmente notables en entornos de alta concurrencia y con grandes volúmenes de datos.

Para obtener el máximo beneficio, es importante seguir las mejores prácticas descritas y monitorear continuamente el rendimiento para identificar nuevas oportunidades de optimización.
