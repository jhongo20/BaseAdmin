# Sistema de Feature Flags

## Descripción

El sistema de Feature Flags (banderas de características) permite activar o desactivar funcionalidades específicas de la aplicación sin necesidad de recompilar o redesplegar el código. Esto es especialmente útil para:

- Realizar pruebas A/B
- Lanzar funcionalidades gradualmente
- Controlar el acceso a funcionalidades en desarrollo o experimentales
- Desactivar temporalmente funcionalidades problemáticas
- Personalizar la experiencia para diferentes entornos (desarrollo, pruebas, producción)

## Arquitectura

El sistema de Feature Flags está compuesto por:

1. **Configuración**: Sección `FeatureFlags` en el archivo `appsettings.json`.
2. **Modelo `FeatureFlags`**: Clase que representa las banderas de características.
3. **Servicio `IFeatureFlagService`**: Interfaz para gestionar las banderas.
4. **Implementación `FeatureFlagService`**: Implementación del servicio.
5. **Controlador `FeatureFlagsController`**: Controlador para gestionar las banderas vía API.
6. **Middleware `FeatureFlagMiddleware`**: Middleware para verificar si las características están habilitadas en cada solicitud.

## Configuración

La configuración del sistema de Feature Flags se realiza en el archivo `appsettings.json`:

```json
{
  "FeatureFlags": {
    "EnableVersionedApi": true,
    "EnableTokenRevocation": true,
    "EnableDistributedSessions": true,
    "EnableMultiFactorAuth": false,
    "EnableLdapIntegration": true,
    "EnablePasswordlessLogin": false,
    "EnableSocialLogin": false,
    "EnableUserSelfRegistration": true,
    "EnableAdvancedPermissions": false,
    "EnableAuditLogging": true,
    "EnableRateLimiting": true,
    "EnableSecurityHeaders": true,
    "EnableSwagger": true
  }
}
```

## Banderas disponibles

| Bandera | Descripción | Valor predeterminado |
|---------|-------------|---------------------|
| `EnableVersionedApi` | Habilita o deshabilita la API versionada | `true` |
| `EnableTokenRevocation` | Habilita o deshabilita el sistema de revocación de tokens | `true` |
| `EnableDistributedSessions` | Habilita o deshabilita el sistema de sesiones distribuidas | `true` |
| `EnableMultiFactorAuth` | Habilita o deshabilita la autenticación de múltiples factores | `false` |
| `EnableLdapIntegration` | Habilita o deshabilita la integración con LDAP | `true` |
| `EnablePasswordlessLogin` | Habilita o deshabilita el inicio de sesión sin contraseña | `false` |
| `EnableSocialLogin` | Habilita o deshabilita el inicio de sesión con redes sociales | `false` |
| `EnableUserSelfRegistration` | Habilita o deshabilita el auto-registro de usuarios | `true` |
| `EnableAdvancedPermissions` | Habilita o deshabilita el sistema avanzado de permisos | `false` |
| `EnableAuditLogging` | Habilita o deshabilita el registro de auditoría | `true` |
| `EnableRateLimiting` | Habilita o deshabilita la limitación de solicitudes | `true` |
| `EnableSecurityHeaders` | Habilita o deshabilita los encabezados de seguridad | `true` |
| `EnableSwagger` | Habilita o deshabilita Swagger | `true` |

## Endpoints

### Obtener todas las banderas de características

```
GET /api/v1/feature-flags
```

Devuelve todas las banderas de características y sus valores actuales.

**Respuesta exitosa (200 OK):**
```json
{
  "EnableVersionedApi": true,
  "EnableTokenRevocation": true,
  "EnableDistributedSessions": true,
  "EnableMultiFactorAuth": false,
  "EnableLdapIntegration": true,
  "EnablePasswordlessLogin": false,
  "EnableSocialLogin": false,
  "EnableUserSelfRegistration": true,
  "EnableAdvancedPermissions": false,
  "EnableAuditLogging": true,
  "EnableRateLimiting": true,
  "EnableSecurityHeaders": true,
  "EnableSwagger": true
}
```

### Obtener una bandera específica

```
GET /api/v1/feature-flags/{featureName}
```

Devuelve el valor actual de una bandera específica.

**Parámetros de ruta:**
- `featureName`: Nombre de la bandera.

**Respuesta exitosa (200 OK):**
```json
{
  "featureName": "EnableVersionedApi",
  "isEnabled": true
}
```

### Actualizar una bandera

```
PUT /api/v1/feature-flags/{featureName}
```

Actualiza el valor de una bandera específica.

**Parámetros de ruta:**
- `featureName`: Nombre de la bandera.

**Cuerpo de la solicitud:**
```json
{
  "isEnabled": false
}
```

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Característica 'EnableVersionedApi' actualizada correctamente",
  "isEnabled": false
}
```

### Recargar la configuración

```
POST /api/v1/feature-flags/reload
```

Recarga la configuración de las banderas desde el archivo `appsettings.json`.

**Respuesta exitosa (200 OK):**
```json
{
  "message": "Configuración de banderas de características recargada correctamente"
}
```

## Uso en el código

### Verificar si una característica está habilitada

```csharp
// En un controlador o servicio
public class MyController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlagService;
    
    public MyController(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }
    
    [HttpGet]
    public IActionResult MyAction()
    {
        if (_featureFlagService.IsFeatureEnabled("EnableMultiFactorAuth"))
        {
            // Implementar lógica para MFA
        }
        else
        {
            // Implementar lógica alternativa
        }
        
        return Ok();
    }
}
```

### Middleware

El middleware `FeatureFlagMiddleware` verifica automáticamente si las características requeridas están habilitadas para cada solicitud. Si una característica está deshabilitada, el middleware devolverá un error 404 Not Found.

Por ejemplo, si `EnableVersionedApi` está deshabilitado, todas las solicitudes a `/api/v1/*` y `/api/v2/*` serán rechazadas.

## Consideraciones de seguridad

- Los endpoints de gestión de Feature Flags solo están disponibles para usuarios con el rol `Admin`.
- Los cambios en las banderas se aplican inmediatamente, sin necesidad de reiniciar la aplicación.
- Los cambios realizados a través de la API no se guardan en el archivo `appsettings.json`, por lo que se perderán al reiniciar la aplicación.
- Para cambios permanentes, se recomienda modificar directamente el archivo `appsettings.json`.

## Ejemplo de uso

### Desactivar temporalmente una funcionalidad problemática

Si se detecta un problema con el sistema de revocación de tokens, se puede desactivar temporalmente:

```http
PUT /api/v1/feature-flags/EnableTokenRevocation
Content-Type: application/json

{
  "isEnabled": false
}
```

### Activar una funcionalidad experimental para pruebas

Para activar la autenticación de múltiples factores para pruebas:

```http
PUT /api/v1/feature-flags/EnableMultiFactorAuth
Content-Type: application/json

{
  "isEnabled": true
}
```

### Verificar el estado de todas las funcionalidades

```http
GET /api/v1/feature-flags
```

## Extensibilidad

Para añadir nuevas banderas de características:

1. Añadir la nueva bandera en el archivo `appsettings.json` en la sección `FeatureFlags`.
2. Añadir la propiedad correspondiente en la clase `FeatureFlags`.
3. Actualizar el middleware `FeatureFlagMiddleware` si es necesario para verificar la nueva bandera.
