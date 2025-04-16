# Documentación de la API

Este documento detalla los endpoints disponibles en la API del sistema de autenticación.

## Autenticación

### Login

- **Endpoint**: `POST /api/auth/login`
- **Descripción**: Autentica a un usuario y genera tokens JWT
- **Cuerpo de la solicitud**:
  ```json
  {
    "username": "string",
    "password": "string",
    "useActiveDirectory": true|false
  }
  ```
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "token": "string",
    "refreshToken": "string",
    "expiresIn": 3600,
    "userId": "guid",
    "username": "string",
    "email": "string",
    "roles": ["string"],
    "permissions": ["string"]
  }
  ```

### Refresh Token

- **Endpoint**: `POST /api/auth/refresh`
- **Descripción**: Renueva un token JWT expirado usando un refresh token
- **Cuerpo de la solicitud**:
  ```json
  {
    "refreshToken": "string"
  }
  ```
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "token": "string",
    "refreshToken": "string",
    "expiresIn": 3600
  }
  ```

### Logout

- **Endpoint**: `POST /api/auth/logout`
- **Descripción**: Cierra la sesión del usuario y revoca el token
- **Cuerpo de la solicitud**: Ninguno
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "message": "Sesión cerrada correctamente"
  }
  ```

## Gestión de Contraseñas

### Solicitar Restablecimiento de Contraseña

- **Endpoint**: `POST /api/password/reset-request`
- **Descripción**: Solicita un token para restablecer la contraseña
- **Cuerpo de la solicitud**:
  ```json
  {
    "email": "string"
  }
  ```
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "message": "Si el correo está registrado, recibirás instrucciones para restablecer tu contraseña"
  }
  ```

### Confirmar Restablecimiento de Contraseña

- **Endpoint**: `POST /api/password/reset-confirm`
- **Descripción**: Restablece la contraseña utilizando un token
- **Cuerpo de la solicitud**:
  ```json
  {
    "token": "string",
    "newPassword": "string",
    "confirmPassword": "string"
  }
  ```
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "message": "Contraseña restablecida correctamente"
  }
  ```

### Cambiar Contraseña

- **Endpoint**: `POST /api/password/change`
- **Descripción**: Cambia la contraseña del usuario autenticado
- **Encabezados**: `Authorization: Bearer {token}`
- **Cuerpo de la solicitud**:
  ```json
  {
    "currentPassword": "string",
    "newPassword": "string",
    "confirmPassword": "string"
  }
  ```
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "message": "Contraseña cambiada correctamente"
  }
  ```

## Gestión de Cuentas

### Activar Cuenta

- **Endpoint**: `POST /api/account/activate`
- **Descripción**: Activa una cuenta de usuario usando un token
- **Cuerpo de la solicitud**:
  ```json
  {
    "token": "string"
  }
  ```
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "message": "Cuenta activada correctamente"
  }
  ```

### Reenviar Correo de Activación

- **Endpoint**: `POST /api/account/resend-activation`
- **Descripción**: Reenvía el correo de activación
- **Cuerpo de la solicitud**:
  ```json
  "email@example.com"
  ```
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "message": "Si el correo está registrado y la cuenta no está activada, recibirás un nuevo correo de activación"
  }
  ```

## Códigos de Error Comunes

- **400 Bad Request**: Solicitud inválida o datos faltantes
- **401 Unauthorized**: Credenciales inválidas o token expirado
- **403 Forbidden**: No tiene permisos para acceder al recurso
- **404 Not Found**: Recurso no encontrado
- **429 Too Many Requests**: Demasiadas solicitudes (rate limiting)
- **500 Internal Server Error**: Error interno del servidor

## Notas de Seguridad

- Todas las contraseñas deben cumplir con la política de seguridad (mínimo 8 caracteres, al menos una letra minúscula, una mayúscula, un número y un carácter especial)
- Los tokens JWT tienen una duración de 60 minutos por defecto
- Los refresh tokens tienen una duración de 7 días por defecto
- Los tokens de restablecimiento de contraseña y activación de cuenta expiran después de 24 horas
- Se aplica rate limiting para proteger contra ataques de fuerza bruta
