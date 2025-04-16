# Características de Seguridad

Este documento detalla las características de seguridad implementadas en el sistema de autenticación.

## Middleware de Auditoría

El sistema incluye un middleware de auditoría que registra automáticamente las acciones sensibles realizadas en el sistema:

- **Rutas Auditadas**: 
  - `/api/auth/login`: Intentos de inicio de sesión
  - `/api/users`: Operaciones sobre usuarios
  - `/api/roles`: Operaciones sobre roles
  - `/api/permissions`: Operaciones sobre permisos

- **Información Registrada**:
  - Método HTTP
  - Ruta accedida
  - Parámetros de consulta
  - Cuerpo de la solicitud (sanitizado)
  - Cuerpo de la respuesta (sanitizado)
  - Código de estado HTTP
  - Dirección IP del cliente
  - User-Agent
  - ID del usuario (si está autenticado)
  - Fecha y hora

- **Sanitización de Datos**: 
  - Se eliminan contraseñas, tokens y otra información sensible antes de almacenar los datos.

## Encabezados de Seguridad

Se implementó un middleware que agrega automáticamente encabezados de seguridad a todas las respuestas:

- **Content-Security-Policy**: Mitiga ataques XSS controlando qué recursos puede cargar el navegador.
- **X-Content-Type-Options**: Evita que el navegador intente adivinar el tipo MIME.
- **X-Frame-Options**: Protege contra ataques de clickjacking.
- **X-XSS-Protection**: Activa el filtro XSS en navegadores antiguos.
- **Referrer-Policy**: Controla qué información de referencia se incluye con las solicitudes.
- **Strict-Transport-Security**: Fuerza conexiones HTTPS.
- **Permissions-Policy**: Controla qué características y APIs puede usar el sitio.

## Gestión de Contraseñas

El sistema proporciona funcionalidades completas para la gestión segura de contraseñas:

### Restablecimiento de Contraseña

1. **Solicitud de Restablecimiento**:
   - Endpoint: `POST /api/password/reset-request`
   - Cuerpo: `{ "email": "usuario@ejemplo.com" }`
   - Genera un token JWT de propósito específico con expiración de 24 horas
   - Envía un correo electrónico con un enlace para restablecer la contraseña

2. **Confirmación de Restablecimiento**:
   - Endpoint: `POST /api/password/reset-confirm`
   - Cuerpo: `{ "token": "...", "newPassword": "...", "confirmPassword": "..." }`
   - Valida el token y actualiza la contraseña
   - Envía una notificación de seguridad por correo electrónico

### Cambio de Contraseña (Usuario Autenticado)

- Endpoint: `POST /api/password/change`
- Cuerpo: `{ "currentPassword": "...", "newPassword": "...", "confirmPassword": "..." }`
- Requiere autenticación
- Valida la contraseña actual antes de permitir el cambio
- Envía una notificación de seguridad por correo electrónico

### Políticas de Contraseñas

Las contraseñas deben cumplir con los siguientes requisitos:
- Mínimo 8 caracteres
- Al menos una letra minúscula
- Al menos una letra mayúscula
- Al menos un número
- Al menos un carácter especial

## Activación de Cuentas

El sistema incluye funcionalidades para la activación de cuentas de usuario:

1. **Activación de Cuenta**:
   - Endpoint: `POST /api/account/activate`
   - Cuerpo: `{ "token": "..." }`
   - Valida el token y activa la cuenta
   - Envía un correo de bienvenida

2. **Reenvío de Correo de Activación**:
   - Endpoint: `POST /api/account/resend-activation`
   - Cuerpo: `"usuario@ejemplo.com"`
   - Genera un nuevo token y envía un correo de activación

## Servicio de Correo Electrónico

Se implementó un servicio completo para el envío de correos electrónicos:

- **Plantillas Predefinidas**:
  - Restablecimiento de contraseña
  - Activación de cuenta
  - Alertas de seguridad

- **Características**:
  - Soporte para HTML
  - Archivos adjuntos
  - CC y BCC
  - Envío a múltiples destinatarios

## Tokens JWT de Propósito Específico

Se implementaron tokens JWT especializados para diferentes propósitos:

- **Token de Restablecimiento de Contraseña**:
  - Propósito: "PasswordReset"
  - Duración: 24 horas
  - Incluye ID de usuario

- **Token de Activación de Cuenta**:
  - Propósito: "AccountActivation"
  - Duración: 24 horas
  - Incluye ID de usuario

Estos tokens utilizan la misma infraestructura JWT pero con validaciones específicas para cada propósito.
