# Sistema de Sesiones Distribuidas

## Descripción

El sistema de sesiones distribuidas permite gestionar las sesiones de usuario de manera centralizada y escalable, facilitando la implementación de funcionalidades como:

- Cierre forzado de sesiones en eventos críticos
- Limitación del número de sesiones concurrentes por usuario
- Monitoreo en tiempo real de sesiones activas
- Sincronización de estado de sesión entre múltiples instancias de la aplicación
- Persistencia de sesiones incluso después de reinicios del servidor

## Arquitectura

El sistema de sesiones distribuidas está compuesto por:

1. **Almacenamiento de Sesiones**: Las sesiones se almacenan en la base de datos SQL Server y se pueden cachear en Redis para un acceso más rápido.
2. **Entidad `UserSession`**: Almacena información detallada sobre cada sesión de usuario.
3. **Repositorio `IUserSessionRepository`**: Proporciona métodos para gestionar las sesiones de usuario.
4. **Servicio `ISessionManagementService`**: Implementa la lógica de negocio para gestionar sesiones.
5. **Middleware `SessionValidationMiddleware`**: Verifica y actualiza el estado de la sesión en cada solicitud.
6. **Controlador `SessionsController`**: Expone endpoints para gestionar sesiones.

## Configuración

La configuración del sistema de sesiones distribuidas se realiza en el archivo `appsettings.json`:

```json
{
  "SessionManagement": {
    "MaxConcurrentSessions": 5,
    "SessionTimeoutMinutes": 30,
    "EnableSessionPersistence": true,
    "EnableRedisCache": true,
    "ForceLogoutOnPasswordChange": true,
    "ForceLogoutOnRoleChange": true,
    "TrackUserActivity": true,
    "ActivityTrackingIntervalSeconds": 60
  }
}
```

## Endpoints

### Obtener sesiones activas del usuario actual

```
GET /api/sessions/current
```

Devuelve información sobre todas las sesiones activas del usuario autenticado.

**Respuesta exitosa (200 OK):**
```json
{
  "sessions": [
    {
      "id": "12345678-1234-1234-1234-123456789012",
      "createdAt": "2025-04-16T10:30:45Z",
      "lastActivity": "2025-04-16T15:20:10Z",
      "ipAddress": "192.168.1.1",
      "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) ...",
      "deviceInfo": "Windows 10, Chrome 120",
      "isCurrent": true
    },
    {
      "id": "87654321-4321-4321-4321-210987654321",
      "createdAt": "2025-04-15T08:15:30Z",
      "lastActivity": "2025-04-16T14:05:22Z",
      "ipAddress": "192.168.1.100",
      "userAgent": "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) ...",
      "deviceInfo": "iPhone, Safari",
      "isCurrent": false
    }
  ],
  "totalCount": 2,
  "maxAllowed": 5
}
```

### Cerrar una sesión específica

```
DELETE /api/sessions/{sessionId}
```

Cierra una sesión específica del usuario autenticado.

**Parámetros de ruta:**
- `sessionId`: ID de la sesión a cerrar.

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Sesión cerrada correctamente"
}
```

### Cerrar todas las sesiones excepto la actual

```
DELETE /api/sessions/all-except-current
```

Cierra todas las sesiones del usuario autenticado excepto la sesión actual.

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Se han cerrado 3 sesiones correctamente"
}
```

### Cerrar todas las sesiones de un usuario (solo para administradores)

```
DELETE /api/sessions/user/{userId}
```

Cierra todas las sesiones de un usuario específico.

**Parámetros de ruta:**
- `userId`: ID del usuario cuyas sesiones se cerrarán.

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Se han cerrado 5 sesiones del usuario correctamente"
}
```

### Obtener todas las sesiones activas (solo para administradores)

```
GET /api/sessions/all
```

Devuelve información sobre todas las sesiones activas en el sistema.

**Parámetros de consulta:**
- `page` (opcional): Número de página. Por defecto: 1.
- `pageSize` (opcional): Tamaño de página. Por defecto: 20.
- `sortBy` (opcional): Campo por el que ordenar. Por defecto: "lastActivity".
- `sortDirection` (opcional): Dirección de ordenación. Por defecto: "desc".

**Respuesta exitosa (200 OK):**
```json
{
  "sessions": [
    {
      "id": "12345678-1234-1234-1234-123456789012",
      "userId": "98765432-9876-9876-9876-987654321098",
      "username": "john.doe",
      "createdAt": "2025-04-16T10:30:45Z",
      "lastActivity": "2025-04-16T15:20:10Z",
      "ipAddress": "192.168.1.1",
      "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) ...",
      "deviceInfo": "Windows 10, Chrome 120"
    },
    // ... más sesiones
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8
}
```

## Integración con el sistema de autenticación

El sistema de sesiones distribuidas se integra con el sistema de autenticación de la siguiente manera:

1. **Creación de sesión**: Cuando un usuario inicia sesión, se crea una nueva entrada en la tabla `UserSessions`.
2. **Validación de sesión**: En cada solicitud, el middleware `SessionValidationMiddleware` verifica que la sesión asociada al token JWT sea válida.
3. **Actualización de actividad**: Cada vez que un usuario realiza una acción, se actualiza el campo `LastActivity` de su sesión.
4. **Cierre de sesión**: Cuando un usuario cierra sesión, se marca su sesión como inactiva y se revoca el token JWT asociado.

## Consideraciones de rendimiento

- Las consultas frecuentes a la tabla `UserSessions` pueden afectar el rendimiento, por lo que se recomienda utilizar Redis como caché.
- La actualización del campo `LastActivity` se realiza con una frecuencia configurable para evitar sobrecarga de la base de datos.
- Se recomienda configurar un trabajo programado para limpiar las sesiones expiradas periódicamente.

## Ejemplo de uso

### Monitoreo de sesiones activas

```csharp
// Controlador de dashboard de administrador
[HttpGet("dashboard/active-sessions")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> GetActiveSessionsStats()
{
    var totalSessions = await _sessionManagementService.GetTotalActiveSessionsAsync();
    var sessionsByRole = await _sessionManagementService.GetActiveSessionsByRoleAsync();
    var sessionsLast24Hours = await _sessionManagementService.GetNewSessionsInLastHoursAsync(24);
    
    return Ok(new
    {
        totalActiveSessions = totalSessions,
        sessionsByRole = sessionsByRole,
        newSessionsLast24Hours = sessionsLast24Hours
    });
}
```

### Forzar cierre de sesión en cambio de rol

```csharp
// Servicio de gestión de roles
public async Task UpdateUserRoleAsync(Guid userId, Guid roleId)
{
    // Actualizar rol del usuario...
    
    // Forzar cierre de todas las sesiones del usuario
    await _sessionManagementService.CloseAllUserSessionsAsync(
        userId,
        "Cambio de rol",
        GetCurrentUserId()
    );
    
    // Notificar al usuario
    await _notificationService.SendRoleChangeNotificationAsync(userId, roleId);
}
```
