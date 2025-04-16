# Documentación de Bloqueo de Cuentas en AuthSystem

## Introducción

El sistema de bloqueo de cuentas es una característica de seguridad que protege contra ataques de fuerza bruta y accesos no autorizados. Cuando un usuario falla repetidamente al intentar iniciar sesión, su cuenta se bloquea temporalmente para prevenir intentos adicionales.

## Características Principales

- **Bloqueo automático**: Después de un número configurable de intentos fallidos, la cuenta se bloquea temporalmente
- **Duración configurable**: El tiempo de bloqueo se puede ajustar según las necesidades de seguridad
- **Desbloqueo automático**: Las cuentas se desbloquean automáticamente después del período de bloqueo
- **Desbloqueo manual**: Los administradores pueden desbloquear cuentas antes de que expire el período de bloqueo
- **Reinicio de contador**: El contador de intentos fallidos se reinicia después de un inicio de sesión exitoso
- **Registro de eventos**: Todos los intentos fallidos y bloqueos se registran para auditoría

## Configuración

La configuración del bloqueo de cuentas se encuentra en el archivo `appsettings.json` bajo la sección `AccountLockout`:

```json
"AccountLockout": {
  "MaxFailedAttempts": 5,
  "LockoutDurationMinutes": 15,
  "EnableAccountLockout": true
}
```

### Parámetros de Configuración

| Parámetro | Descripción | Valor Predeterminado |
|-----------|-------------|---------------------|
| `MaxFailedAttempts` | Número máximo de intentos fallidos antes de bloquear la cuenta | 5 |
| `LockoutDurationMinutes` | Duración del bloqueo en minutos | 15 |
| `EnableAccountLockout` | Habilita o deshabilita la funcionalidad de bloqueo de cuentas | true |

## Funcionamiento

### Proceso de Bloqueo

1. Cuando un usuario intenta iniciar sesión con credenciales incorrectas, el sistema incrementa el contador de intentos fallidos (`AccessFailedCount`).
2. Si el contador alcanza o supera el valor de `MaxFailedAttempts`, la cuenta se bloquea estableciendo:
   - `LockoutEnd` a la fecha y hora actual más `LockoutDurationMinutes`
   - `Status` a `Locked`
3. Durante el período de bloqueo, cualquier intento de inicio de sesión será rechazado con un mensaje indicando el tiempo restante.

### Proceso de Desbloqueo

La cuenta puede desbloquearse de tres formas:

1. **Desbloqueo automático por tiempo**: Cuando el período de bloqueo expira (`LockoutEnd` < tiempo actual)
2. **Desbloqueo por inicio de sesión exitoso**: Si las credenciales son correctas después de que expire el bloqueo
3. **Desbloqueo manual por administrador**: A través del endpoint de desbloqueo

## API de Administración

El sistema proporciona endpoints para administrar el bloqueo de cuentas:

### Consultar Estado de Bloqueo

```
GET /api/account-lockout/status/{username}
```

**Respuesta**:
```json
{
  "username": "john.doe",
  "isLocked": true,
  "accessFailedCount": 5,
  "lockoutEnd": "2025-04-16T17:30:00Z",
  "remainingMinutes": 10.5
}
```

### Desbloquear Cuenta

```
POST /api/account-lockout/unlock/{username}
```

**Respuesta**:
```json
{
  "message": "La cuenta del usuario 'john.doe' ha sido desbloqueada correctamente"
}
```

> **Nota**: Estos endpoints requieren autenticación y el rol de `Admin`.

## API de Prueba

Para facilitar las pruebas del sistema de bloqueo de cuentas, se proporciona un controlador específico:

### Simular Intento Fallido

```
POST /api/lockout-test/failed-attempt/{username}
```

**Respuesta**:
```json
{
  "username": "john.doe",
  "accessFailedCount": 3,
  "isLocked": false,
  "lockoutEnd": null,
  "remainingAttempts": 2
}
```

### Simular Inicio de Sesión Exitoso

```
POST /api/lockout-test/successful-login/{username}
```

**Respuesta**:
```json
{
  "username": "john.doe",
  "accessFailedCount": 0,
  "isLocked": false,
  "lastLoginAt": "2025-04-16T17:20:00Z"
}
```

### Verificar Estado de Bloqueo

```
GET /api/lockout-test/status/{username}
```

**Respuesta**: (Igual que el endpoint de administración)

> **Nota**: Estos endpoints de prueba no requieren autenticación y están diseñados para entornos de desarrollo. Deben ser deshabilitados en producción.

## Integración con el Proceso de Inicio de Sesión

El sistema de bloqueo de cuentas está integrado en el proceso de inicio de sesión:

1. **Verificación inicial**: Antes de validar las credenciales, se verifica si la cuenta está bloqueada
2. **Registro de intentos fallidos**: Si las credenciales son incorrectas, se incrementa el contador
3. **Registro de inicios de sesión exitosos**: Si las credenciales son correctas, se reinicia el contador

## Consideraciones de Seguridad

- **Balance entre seguridad y usabilidad**: Ajustar `MaxFailedAttempts` y `LockoutDurationMinutes` según las necesidades
- **Notificaciones**: Considerar notificar a los usuarios cuando sus cuentas sean bloqueadas
- **Monitoreo**: Revisar regularmente los logs para detectar patrones de intentos de acceso no autorizados
- **Protección adicional**: Combinar con otras medidas como CAPTCHA o autenticación de dos factores

## Ejemplo de Uso

### Escenario: Intentos Fallidos y Bloqueo

1. Un usuario intenta iniciar sesión con una contraseña incorrecta 5 veces consecutivas
2. En el 5º intento, el sistema bloquea la cuenta por 15 minutos
3. El usuario recibe un mensaje: "Account is locked. Please try again after 15 minute(s)"
4. Después de 15 minutos, el usuario puede intentar iniciar sesión nuevamente

### Escenario: Desbloqueo por Administrador

1. Un usuario reporta que su cuenta está bloqueada
2. El administrador accede a `GET /api/account-lockout/status/{username}` para verificar
3. El administrador ejecuta `POST /api/account-lockout/unlock/{username}` para desbloquear la cuenta
4. El usuario puede iniciar sesión inmediatamente

## Buenas Prácticas

1. **Monitoreo de patrones**: Revisar regularmente los logs para detectar posibles ataques
2. **Ajuste de parámetros**: Modificar la configuración según el análisis de los intentos fallidos
3. **Educación de usuarios**: Informar a los usuarios sobre esta característica de seguridad
4. **Política de contraseñas**: Combinar con una política de contraseñas robusta
5. **Notificaciones**: Implementar notificaciones por email cuando una cuenta se bloquee

## Solución de Problemas

| Problema | Posible Causa | Solución |
|----------|---------------|----------|
| Bloqueos frecuentes de usuarios legítimos | `MaxFailedAttempts` demasiado bajo | Aumentar el valor en la configuración |
| Bloqueos de larga duración | `LockoutDurationMinutes` demasiado alto | Reducir el valor en la configuración |
| Cuenta no se bloquea | `EnableAccountLockout` en false o `LockoutEnabled` del usuario en false | Verificar la configuración y el estado del usuario |
| Desbloqueo no funciona | Permisos insuficientes | Verificar que el usuario tenga el rol de Admin |
