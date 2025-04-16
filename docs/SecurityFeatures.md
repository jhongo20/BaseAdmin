# Características de Seguridad Avanzadas en AuthSystem

## Introducción

Este documento describe las características de seguridad avanzadas implementadas en AuthSystem para proteger contra accesos no autorizados, ataques de fuerza bruta y otras amenazas de seguridad. Estas características trabajan en conjunto para proporcionar múltiples capas de protección.

## Índice

1. [Bloqueo de Cuentas](#bloqueo-de-cuentas)
2. [Notificaciones por Email](#notificaciones-por-email)
3. [CAPTCHA](#captcha)
4. [Autenticación de Dos Factores](#autenticación-de-dos-factores)
5. [Monitoreo de Seguridad](#monitoreo-de-seguridad)
6. [Integración entre Componentes](#integración-entre-componentes)
7. [Configuración](#configuración)

## Bloqueo de Cuentas

El sistema de bloqueo de cuentas protege contra ataques de fuerza bruta bloqueando temporalmente las cuentas después de múltiples intentos fallidos de inicio de sesión.

### Características

- **Bloqueo automático**: Después de un número configurable de intentos fallidos (predeterminado: 5)
- **Duración configurable**: Tiempo de bloqueo personalizable (predeterminado: 15 minutos)
- **Desbloqueo automático**: Las cuentas se desbloquean automáticamente después del período de bloqueo
- **Desbloqueo manual**: Los administradores pueden desbloquear cuentas antes de que expire el período
- **Reinicio de contador**: El contador de intentos fallidos se reinicia después de un inicio de sesión exitoso

### API de Administración

- `GET /api/account-lockout/users`: Lista todos los usuarios con su estado de bloqueo
- `GET /api/account-lockout/status/{username}`: Consulta el estado de bloqueo de un usuario específico
- `POST /api/account-lockout/unlock/{username}`: Desbloquea manualmente una cuenta

### API de Prueba

- `POST /api/lockout-test/failed-attempt/{username}`: Simula un intento fallido de inicio de sesión
- `POST /api/lockout-test/successful-login/{username}`: Simula un inicio de sesión exitoso
- `GET /api/lockout-test/status/{username}`: Verifica el estado de bloqueo de una cuenta

## Notificaciones por Email

El sistema envía notificaciones por email a los usuarios cuando sus cuentas son bloqueadas, proporcionando información sobre el bloqueo y pasos a seguir.

### Características

- **Notificaciones automáticas**: Se envían emails cuando una cuenta es bloqueada
- **Información detallada**: Incluye detalles sobre el motivo del bloqueo, duración y pasos a seguir
- **Diseño responsivo**: Emails con diseño HTML moderno y legible en cualquier dispositivo
- **Personalización**: Los mensajes incluyen el nombre del usuario y detalles específicos

### Contenido del Email

- Información sobre el bloqueo (fecha, hora, duración)
- Instrucciones para desbloquear la cuenta
- Recomendaciones de seguridad
- Información de contacto para soporte

## CAPTCHA

El sistema implementa CAPTCHA para añadir una capa adicional de seguridad después de cierto número de intentos fallidos de inicio de sesión.

### Características

- **CAPTCHA adaptativo**: Se muestra solo después de un número configurable de intentos fallidos
- **Generación dinámica**: Cada CAPTCHA es único y generado dinámicamente
- **Validación segura**: Verificación del lado del servidor
- **Expiración**: Los CAPTCHAs expiran después de un tiempo determinado
- **Accesibilidad**: Diseñado para ser legible por humanos pero difícil para bots

### Funcionamiento

1. Después de N intentos fallidos (configurable, predeterminado: 2), se requiere CAPTCHA
2. El usuario debe ingresar correctamente el código mostrado en la imagen
3. Si el CAPTCHA es incorrecto, se genera uno nuevo
4. El contador de CAPTCHA se reinicia después de un inicio de sesión exitoso

## Autenticación de Dos Factores

La autenticación de dos factores (2FA) proporciona una capa adicional de seguridad requiriendo un segundo factor de autenticación además de la contraseña.

### Características

- **Basado en TOTP**: Implementa el algoritmo Time-based One-Time Password
- **Compatible con aplicaciones estándar**: Funciona con Google Authenticator, Microsoft Authenticator, Authy, etc.
- **Códigos de recuperación**: Proporciona códigos de recuperación para acceso de emergencia
- **Configuración sencilla**: Proceso de activación intuitivo con código QR
- **Opcional**: Los usuarios pueden habilitar o deshabilitar 2FA según sus preferencias

### API

- `POST /api/two-factor/setup`: Genera la información de configuración inicial
- `POST /api/two-factor/enable`: Habilita 2FA después de verificar el código
- `POST /api/two-factor/disable`: Deshabilita 2FA para el usuario actual
- `POST /api/two-factor/verify`: Verifica un código durante el proceso de inicio de sesión

### Flujo de Autenticación con 2FA

1. El usuario ingresa sus credenciales (usuario y contraseña)
2. Si las credenciales son correctas y 2FA está habilitado, se genera un token de sesión temporal
3. El usuario debe proporcionar un código de verificación de su aplicación de autenticación
4. Si el código es válido, se completa el inicio de sesión y se generan los tokens JWT

## Monitoreo de Seguridad

El sistema incluye un módulo de monitoreo de seguridad que detecta y alerta sobre patrones sospechosos de actividad.

### Características

- **Detección de patrones**: Identifica comportamientos sospechosos como múltiples intentos fallidos
- **Alertas en tiempo real**: Genera alertas para actividades potencialmente maliciosas
- **Estadísticas de seguridad**: Proporciona métricas y tendencias de seguridad
- **Notificaciones**: Alerta al equipo de seguridad sobre incidentes críticos
- **Registro detallado**: Mantiene un historial completo de eventos de seguridad

### Tipos de Alertas

- **Múltiples intentos fallidos**: Detecta varios intentos fallidos para un mismo usuario
- **Múltiples cuentas desde la misma IP**: Identifica intentos en diferentes cuentas desde una IP
- **Posible ataque de fuerza bruta**: Detecta patrones de alta frecuencia de intentos
- **Ubicación inusual**: Alerta sobre inicios de sesión desde ubicaciones no habituales
- **Horario inusual**: Detecta actividad en horarios atípicos para el usuario

### API de Monitoreo

- `GET /api/security-monitoring/alerts`: Obtiene las alertas de seguridad recientes
- `GET /api/security-monitoring/stats`: Obtiene estadísticas de seguridad
- `POST /api/security-monitoring/alerts/{id}/review`: Marca una alerta como revisada
- `POST /api/security-monitoring/detect`: Ejecuta manualmente la detección de patrones

## Integración entre Componentes

Las diferentes características de seguridad están integradas para trabajar en conjunto:

1. **Bloqueo de Cuentas + Notificaciones**: Cuando una cuenta se bloquea, se envía una notificación por email
2. **Bloqueo de Cuentas + CAPTCHA**: Después de ciertos intentos fallidos, se muestra CAPTCHA antes del bloqueo
3. **Monitoreo + Bloqueo**: El sistema de monitoreo registra los intentos fallidos y bloqueos
4. **2FA + Bloqueo**: La autenticación de dos factores reduce la probabilidad de bloqueos por credenciales robadas

## Configuración

Todas las características de seguridad son altamente configurables a través del archivo `appsettings.json`:

### Bloqueo de Cuentas

```json
"AccountLockout": {
  "MaxFailedAttempts": 5,
  "LockoutDurationMinutes": 15,
  "EnableAccountLockout": true,
  "EnableEmailNotifications": true,
  "EnableCaptcha": true,
  "CaptchaThreshold": 2
}
```

### Monitoreo de Seguridad

```json
"SecurityMonitoring": {
  "FailedLoginThreshold": 5,
  "FailedLoginTimeWindowMinutes": 10,
  "MultipleAccountsThreshold": 3,
  "EnableAlertNotifications": true,
  "SecurityTeamEmail": "security@example.com"
}
```

## Mejores Prácticas

1. **Configuración adecuada**: Ajustar los umbrales según las necesidades específicas
2. **Monitoreo regular**: Revisar periódicamente las alertas y estadísticas de seguridad
3. **Educación de usuarios**: Informar a los usuarios sobre estas características y su propósito
4. **Pruebas periódicas**: Realizar pruebas de penetración para verificar la efectividad
5. **Actualizaciones**: Mantener el sistema actualizado con las últimas correcciones de seguridad

## Limitaciones y Consideraciones

1. **Falsos positivos**: Algunas configuraciones muy estrictas pueden afectar a usuarios legítimos
2. **Rendimiento**: El monitoreo intensivo puede tener un impacto en el rendimiento
3. **Almacenamiento**: Las alertas y registros requieren espacio de almacenamiento
4. **Privacidad**: Considerar las implicaciones de privacidad al registrar información de los usuarios

## Conclusión

Las características de seguridad implementadas proporcionan un enfoque de defensa en profundidad para proteger el sistema contra diversas amenazas. La combinación de bloqueo de cuentas, CAPTCHA, autenticación de dos factores y monitoreo avanzado crea múltiples capas de seguridad que trabajan juntas para garantizar la protección de las cuentas de usuario y la integridad del sistema.
