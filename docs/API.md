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
    "message": "Si el correo está registrado, se ha enviado un enlace de restablecimiento"
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
    "message": "Si el correo está registrado y la cuenta no está activada, se ha reenviado el correo de activación"
  }
  ```

## Gestión de Módulos

### Obtener Todos los Módulos

- **Endpoint**: `GET /api/modules`
- **Descripción**: Obtiene todos los módulos del sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (200 OK):
  ```json
  [
    {
      "id": "guid",
      "name": "string",
      "description": "string",
      "route": "string",
      "icon": "string",
      "displayOrder": 1,
      "isEnabled": true,
      "parentId": "guid",
      "children": []
    }
  ]
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X GET "http://localhost:5197/api/modules" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

### Obtener Módulo por ID

- **Endpoint**: `GET /api/modules/{id}`
- **Descripción**: Obtiene un módulo específico por su ID
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "route": "string",
    "icon": "string",
    "displayOrder": 1,
    "isEnabled": true,
    "parentId": "guid",
    "children": []
  }
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X GET "http://localhost:5197/api/modules/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

### Crear Módulo

- **Endpoint**: `POST /api/modules`
- **Descripción**: Crea un nuevo módulo en el sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Cuerpo de la solicitud**:
  ```json
  {
    "name": "string",
    "description": "string",
    "route": "string",
    "icon": "string",
    "displayOrder": 1,
    "isEnabled": true,
    "parentId": "guid"
  }
  ```
- **Respuesta exitosa** (201 Created):
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "route": "string",
    "icon": "string",
    "displayOrder": 1,
    "isEnabled": true,
    "parentId": "guid"
  }
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X POST "http://localhost:5197/api/modules" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." -H "Content-Type: application/json" -d '{"name":"Administración","description":"Módulo de administración","route":"/admin","icon":"fa-cog","displayOrder":1,"isEnabled":true}'
  ```

### Actualizar Módulo

- **Endpoint**: `PUT /api/modules/{id}`
- **Descripción**: Actualiza un módulo existente
- **Encabezados**: `Authorization: Bearer {token}`
- **Cuerpo de la solicitud**:
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "route": "string",
    "icon": "string",
    "displayOrder": 1,
    "isEnabled": true,
    "parentId": "guid"
  }
  ```
- **Respuesta exitosa** (204 No Content)
- **Ejemplo de uso**:
  ```bash
  curl -X PUT "http://localhost:5197/api/modules/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." -H "Content-Type: application/json" -d '{"id":"550e8400-e29b-41d4-a716-446655440000","name":"Administración","description":"Módulo de administración actualizado","route":"/admin","icon":"fa-cog","displayOrder":1,"isEnabled":true}'
  ```

### Eliminar Módulo

- **Endpoint**: `DELETE /api/modules/{id}`
- **Descripción**: Elimina un módulo del sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (204 No Content)
- **Ejemplo de uso**:
  ```bash
  curl -X DELETE "http://localhost:5197/api/modules/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

## Gestión de Organizaciones

### Obtener Todas las Organizaciones

- **Endpoint**: `GET /api/organizations`
- **Descripción**: Obtiene todas las organizaciones del sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (200 OK):
  ```json
  [
    {
      "id": "guid",
      "name": "string",
      "description": "string",
      "taxId": "string",
      "address": "string",
      "phone": "string",
      "email": "string",
      "website": "string",
      "isActive": true
    }
  ]
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X GET "http://localhost:5197/api/organizations" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

### Obtener Organización por ID

- **Endpoint**: `GET /api/organizations/{id}`
- **Descripción**: Obtiene una organización específica por su ID
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "taxId": "string",
    "address": "string",
    "phone": "string",
    "email": "string",
    "website": "string",
    "isActive": true
  }
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X GET "http://localhost:5197/api/organizations/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

### Crear Organización

- **Endpoint**: `POST /api/organizations`
- **Descripción**: Crea una nueva organización en el sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Cuerpo de la solicitud**:
  ```json
  {
    "name": "string",
    "description": "string",
    "taxId": "string",
    "address": "string",
    "phone": "string",
    "email": "string",
    "website": "string",
    "isActive": true
  }
  ```
- **Respuesta exitosa** (201 Created):
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "taxId": "string",
    "address": "string",
    "phone": "string",
    "email": "string",
    "website": "string",
    "isActive": true
  }
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X POST "http://localhost:5197/api/organizations" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." -H "Content-Type: application/json" -d '{"name":"Empresa ABC","description":"Descripción de la empresa","taxId":"123456789","address":"Calle Principal 123","phone":"123-456-7890","email":"info@empresa.com","website":"www.empresa.com","isActive":true}'
  ```

### Actualizar Organización

- **Endpoint**: `PUT /api/organizations/{id}`
- **Descripción**: Actualiza una organización existente
- **Encabezados**: `Authorization: Bearer {token}`
- **Cuerpo de la solicitud**:
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "taxId": "string",
    "address": "string",
    "phone": "string",
    "email": "string",
    "website": "string",
    "isActive": true
  }
  ```
- **Respuesta exitosa** (204 No Content)
- **Ejemplo de uso**:
  ```bash
  curl -X PUT "http://localhost:5197/api/organizations/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." -H "Content-Type: application/json" -d '{"id":"550e8400-e29b-41d4-a716-446655440000","name":"Empresa ABC Actualizada","description":"Descripción actualizada","taxId":"123456789","address":"Calle Principal 123","phone":"123-456-7890","email":"info@empresa.com","website":"www.empresa.com","isActive":true}'
  ```

### Eliminar Organización

- **Endpoint**: `DELETE /api/organizations/{id}`
- **Descripción**: Elimina una organización del sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (204 No Content)
- **Ejemplo de uso**:
  ```bash
  curl -X DELETE "http://localhost:5197/api/organizations/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

## Gestión de Permisos

### Obtener Todos los Permisos

- **Endpoint**: `GET /api/permissions`
- **Descripción**: Obtiene todos los permisos del sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (200 OK):
  ```json
  [
    {
      "id": "guid",
      "name": "string",
      "description": "string",
      "type": "string",
      "moduleId": "guid",
      "isActive": true
    }
  ]
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X GET "http://localhost:5197/api/permissions" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

### Obtener Permiso por ID

- **Endpoint**: `GET /api/permissions/{id}`
- **Descripción**: Obtiene un permiso específico por su ID
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "type": "string",
    "moduleId": "guid",
    "isActive": true
  }
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X GET "http://localhost:5197/api/permissions/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

### Crear Permiso

- **Endpoint**: `POST /api/permissions`
- **Descripción**: Crea un nuevo permiso en el sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Cuerpo de la solicitud**:
  ```json
  {
    "name": "string",
    "description": "string",
    "type": "string",
    "moduleId": "guid",
    "isActive": true
  }
  ```
- **Respuesta exitosa** (201 Created):
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "type": "string",
    "moduleId": "guid",
    "isActive": true
  }
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X POST "http://localhost:5197/api/permissions" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." -H "Content-Type: application/json" -d '{"name":"CreateUser","description":"Permiso para crear usuarios","type":"Create","moduleId":"550e8400-e29b-41d4-a716-446655440000","isActive":true}'
  ```

### Actualizar Permiso

- **Endpoint**: `PUT /api/permissions/{id}`
- **Descripción**: Actualiza un permiso existente
- **Encabezados**: `Authorization: Bearer {token}`
- **Cuerpo de la solicitud**:
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "type": "string",
    "moduleId": "guid",
    "isActive": true
  }
  ```
- **Respuesta exitosa** (204 No Content)
- **Ejemplo de uso**:
  ```bash
  curl -X PUT "http://localhost:5197/api/permissions/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." -H "Content-Type: application/json" -d '{"id":"550e8400-e29b-41d4-a716-446655440000","name":"CreateUser","description":"Permiso actualizado para crear usuarios","type":"Create","moduleId":"550e8400-e29b-41d4-a716-446655440000","isActive":true}'
  ```

### Eliminar Permiso

- **Endpoint**: `DELETE /api/permissions/{id}`
- **Descripción**: Elimina un permiso del sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (204 No Content)
- **Ejemplo de uso**:
  ```bash
  curl -X DELETE "http://localhost:5197/api/permissions/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

## Gestión de Roles

### Obtener Todos los Roles

- **Endpoint**: `GET /api/roles`
- **Descripción**: Obtiene todos los roles del sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (200 OK):
  ```json
  [
    {
      "id": "guid",
      "name": "string",
      "description": "string",
      "organizationId": "guid",
      "isActive": true
    }
  ]
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X GET "http://localhost:5197/api/roles" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

### Obtener Rol por ID

- **Endpoint**: `GET /api/roles/{id}`
- **Descripción**: Obtiene un rol específico por su ID
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "organizationId": "guid",
    "isActive": true
  }
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X GET "http://localhost:5197/api/roles/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

### Crear Rol

- **Endpoint**: `POST /api/roles`
- **Descripción**: Crea un nuevo rol en el sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Cuerpo de la solicitud**:
  ```json
  {
    "name": "string",
    "description": "string",
    "organizationId": "guid",
    "isActive": true
  }
  ```
- **Respuesta exitosa** (201 Created):
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "organizationId": "guid",
    "isActive": true
  }
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X POST "http://localhost:5197/api/roles" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." -H "Content-Type: application/json" -d '{"name":"Administrador","description":"Rol con acceso total al sistema","organizationId":"550e8400-e29b-41d4-a716-446655440000","isActive":true}'
  ```

### Actualizar Rol

- **Endpoint**: `PUT /api/roles/{id}`
- **Descripción**: Actualiza un rol existente
- **Encabezados**: `Authorization: Bearer {token}`
- **Cuerpo de la solicitud**:
  ```json
  {
    "id": "guid",
    "name": "string",
    "description": "string",
    "organizationId": "guid",
    "isActive": true
  }
  ```
- **Respuesta exitosa** (204 No Content)
- **Ejemplo de uso**:
  ```bash
  curl -X PUT "http://localhost:5197/api/roles/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." -H "Content-Type: application/json" -d '{"id":"550e8400-e29b-41d4-a716-446655440000","name":"Administrador","description":"Rol actualizado con acceso total al sistema","organizationId":"550e8400-e29b-41d4-a716-446655440000","isActive":true}'
  ```

### Eliminar Rol

- **Endpoint**: `DELETE /api/roles/{id}`
- **Descripción**: Elimina un rol del sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (204 No Content)
- **Ejemplo de uso**:
  ```bash
  curl -X DELETE "http://localhost:5197/api/roles/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

## Gestión de Usuarios

### Obtener Todos los Usuarios

- **Endpoint**: `GET /api/users`
- **Descripción**: Obtiene todos los usuarios del sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (200 OK):
  ```json
  [
    {
      "id": "guid",
      "username": "string",
      "email": "string",
      "fullName": "string",
      "userType": "string",
      "isActive": true
    }
  ]
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X GET "http://localhost:5197/api/users" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

### Obtener Usuario por ID

- **Endpoint**: `GET /api/users/{id}`
- **Descripción**: Obtiene un usuario específico por su ID
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (200 OK):
  ```json
  {
    "id": "guid",
    "username": "string",
    "email": "string",
    "fullName": "string",
    "userType": "string",
    "isActive": true
  }
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X GET "http://localhost:5197/api/users/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  ```

### Crear Usuario

- **Endpoint**: `POST /api/users`
- **Descripción**: Crea un nuevo usuario en el sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Cuerpo de la solicitud**:
  ```json
  {
    "username": "string",
    "email": "string",
    "password": "string",
    "fullName": "string",
    "userType": "string",
    "isActive": true
  }
  ```
- **Respuesta exitosa** (201 Created):
  ```json
  {
    "id": "guid",
    "username": "string",
    "email": "string",
    "fullName": "string",
    "userType": "string",
    "isActive": true
  }
  ```
- **Ejemplo de uso**:
  ```bash
  curl -X POST "http://localhost:5197/api/users" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." -H "Content-Type: application/json" -d '{"username":"johndoe","email":"john.doe@example.com","password":"Password123!","fullName":"John Doe","userType":"Local","isActive":true}'
  ```

### Actualizar Usuario

- **Endpoint**: `PUT /api/users/{id}`
- **Descripción**: Actualiza un usuario existente
- **Encabezados**: `Authorization: Bearer {token}`
- **Cuerpo de la solicitud**:
  ```json
  {
    "id": "guid",
    "username": "string",
    "email": "string",
    "fullName": "string",
    "userType": "string",
    "isActive": true
  }
  ```
- **Respuesta exitosa** (204 No Content)
- **Ejemplo de uso**:
  ```bash
  curl -X PUT "http://localhost:5197/api/users/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." -H "Content-Type: application/json" -d '{"id":"550e8400-e29b-41d4-a716-446655440000","username":"johndoe","email":"john.doe@example.com","fullName":"John Doe Updated","userType":"Local","isActive":true}'
  ```

### Eliminar Usuario

- **Endpoint**: `DELETE /api/users/{id}`
- **Descripción**: Elimina un usuario del sistema
- **Encabezados**: `Authorization: Bearer {token}`
- **Respuesta exitosa** (204 No Content)
- **Ejemplo de uso**:
  ```bash
  curl -X DELETE "http://localhost:5197/api/users/550e8400-e29b-41d4-a716-446655440000" -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
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
