# Sistema de Revocación de Tokens

## Descripción

El sistema de revocación de tokens permite invalidar tokens JWT antes de su fecha de expiración natural. Esto es útil en escenarios como:

- Cierre de sesión del usuario
- Cambio de contraseña
- Detección de actividad sospechosa
- Revocación de permisos
- Bloqueo de cuenta

## Arquitectura

El sistema de revocación de tokens está compuesto por:

1. **Entidad `RevokedToken`**: Almacena información sobre los tokens revocados.
2. **Repositorio `IRevokedTokenRepository`**: Proporciona métodos para gestionar los tokens revocados.
3. **Servicio `ITokenRevocationService`**: Implementa la lógica de negocio para revocar tokens.
4. **Middleware `TokenRevocationMiddleware`**: Verifica si los tokens están revocados en cada solicitud.
5. **Controlador `TokenRevocationController`**: Expone endpoints para revocar tokens.

## Endpoints

### Revocar el token actual

```
POST /api/token-revocation/current
```

Revoca el token JWT actual del usuario autenticado.

**Parámetros de consulta:**
- `reason` (opcional): Motivo de la revocación. Por defecto: "Revocado por el usuario".

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Token revocado correctamente"
}
```

### Revocar todos los tokens del usuario actual

```
POST /api/token-revocation/all
```

Revoca todos los tokens JWT activos del usuario autenticado.

**Parámetros de consulta:**
- `reason` (opcional): Motivo de la revocación. Por defecto: "Revocados por el usuario".

**Respuesta exitosa (200 OK):**
```json
{
  "message": "3 tokens revocados correctamente"
}
```

### Revocar todos los tokens de un usuario específico (solo para administradores)

```
POST /api/token-revocation/user/{userId}
```

Revoca todos los tokens JWT activos de un usuario específico.

**Parámetros de ruta:**
- `userId`: ID del usuario cuyos tokens se revocarán.

**Parámetros de consulta:**
- `reason` (opcional): Motivo de la revocación. Por defecto: "Revocados por un administrador".

**Respuesta exitosa (200 OK):**
```json
{
  "message": "5 tokens del usuario 12345678-1234-1234-1234-123456789012 revocados correctamente"
}
```

### Limpiar tokens revocados expirados (solo para administradores)

```
DELETE /api/token-revocation/cleanup
```

Elimina los tokens revocados que ya han expirado.

**Respuesta exitosa (200 OK):**
```json
{
  "message": "10 tokens revocados expirados eliminados correctamente"
}
```

## Integración con el sistema de autenticación

El middleware `TokenRevocationMiddleware` se ejecuta antes de los middleware de autenticación y autorización, verificando si el token JWT proporcionado en la solicitud está revocado. Si el token está revocado, la solicitud es rechazada con un código de estado 401 Unauthorized.

## Consideraciones de seguridad

- Los tokens revocados se almacenan en la base de datos hasta su fecha de expiración.
- Se recomienda ejecutar periódicamente el endpoint `/api/token-revocation/cleanup` para eliminar los tokens revocados expirados.
- El sistema registra la dirección IP y el User-Agent desde donde se revocó el token.
- Se registra el usuario que revocó el token (si aplica).

## Ejemplo de uso

### Revocar el token actual al cerrar sesión

```javascript
// Cliente JavaScript
async function logout() {
  try {
    const response = await fetch('/api/token-revocation/current', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });
    
    if (response.ok) {
      // Eliminar token del almacenamiento local
      localStorage.removeItem('token');
      // Redirigir a la página de inicio de sesión
      window.location.href = '/login';
    }
  } catch (error) {
    console.error('Error al cerrar sesión:', error);
  }
}
```

### Revocar todos los tokens al cambiar la contraseña

```csharp
// Controlador de contraseñas
[HttpPost("change-password")]
[Authorize]
public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
{
    // Cambiar la contraseña...
    
    // Revocar todos los tokens del usuario
    await _tokenRevocationService.RevokeAllUserTokensAsync(
        userId,
        "Cambio de contraseña",
        userId,
        HttpContext.Connection.RemoteIpAddress?.ToString(),
        HttpContext.Request.Headers["User-Agent"]
    );
    
    return Ok(new { message = "Contraseña cambiada correctamente y todas las sesiones cerradas" });
}
```
