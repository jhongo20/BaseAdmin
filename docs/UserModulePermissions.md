# Gestión de Usuarios, Roles y Permisos por Módulo

Este documento describe los endpoints implementados para la gestión avanzada de usuarios, roles y permisos en el sistema, con enfoque en la organización por módulos.

## Índice

1. [Introducción](#introducción)
2. [Endpoints de Usuarios por Módulo](#endpoints-de-usuarios-por-módulo)
   - [Listar Usuarios por Módulo](#listar-usuarios-por-módulo)
   - [Agrupar Usuarios por Módulo](#agrupar-usuarios-por-módulo)
3. [Endpoints de Permisos por Ruta](#endpoints-de-permisos-por-ruta)
   - [Verificar Permisos de Usuario para una Ruta](#verificar-permisos-de-usuario-para-una-ruta)
4. [Endpoints de Gestión de Permisos de Roles](#endpoints-de-gestión-de-permisos-de-roles)
   - [Asignar Permisos a un Rol](#asignar-permisos-a-un-rol)
   - [Eliminar Permiso de un Rol](#eliminar-permiso-de-un-rol)
   - [Actualizar Todos los Permisos de un Rol](#actualizar-todos-los-permisos-de-un-rol)
5. [Modelos de Datos](#modelos-de-datos)
6. [Ejemplos de Uso](#ejemplos-de-uso)
7. [Consideraciones de Seguridad](#consideraciones-de-seguridad)

## Introducción

La gestión de usuarios, roles y permisos es fundamental para cualquier sistema de autenticación y autorización robusto. Esta implementación extiende las capacidades básicas del sistema para permitir:

- Visualizar usuarios por módulo con información detallada
- Agrupar usuarios según los módulos a los que tienen acceso
- Verificar permisos específicos de un usuario para una ruta determinada
- Gestionar la asignación de permisos a roles de manera eficiente

Estos endpoints facilitan la administración del sistema y permiten un control más granular sobre quién puede acceder a qué funcionalidades.

## Endpoints de Usuarios por Módulo

### Listar Usuarios por Módulo

Este endpoint devuelve todos los usuarios que tienen acceso a un módulo específico, incluyendo detalles como sus roles y estado.

**Endpoint:** `GET /api/user-modules/module/{moduleId}`

**Parámetros:**
- `moduleId` (path): ID del módulo (GUID)

**Respuesta:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "username": "usuario1",
    "fullName": "Usuario Ejemplo",
    "email": "usuario@ejemplo.com",
    "status": "Active",
    "userType": "Internal",
    "lastLogin": "2025-04-15T10:30:00Z",
    "roles": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "name": "Administrador",
        "description": "Rol de administrador del sistema",
        "isActive": true,
        "validFrom": "2025-01-01T00:00:00Z",
        "validTo": null
      }
    ],
    "detailInfo": {
      "phoneNumber": "+1234567890",
      "createdAt": "2025-01-01T00:00:00Z",
      "lastModifiedAt": "2025-04-01T00:00:00Z",
      "failedLoginAttempts": 0,
      "isLocked": false,
      "lockoutEnd": null,
      "twoFactorEnabled": true
    }
  }
]
```

**Códigos de respuesta:**
- `200 OK`: Solicitud exitosa
- `404 Not Found`: Módulo no encontrado
- `500 Internal Server Error`: Error del servidor

### Agrupar Usuarios por Módulo

Este endpoint devuelve todos los módulos del sistema con los usuarios que tienen acceso a cada uno, agrupados por módulo.

**Endpoint:** `GET /api/user-modules/grouped`

**Respuesta:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Administración",
    "description": "Módulo de administración del sistema",
    "route": "/admin",
    "icon": "fa-cog",
    "isEnabled": true,
    "userCount": 2,
    "users": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "username": "usuario1",
        "fullName": "Usuario Ejemplo",
        "email": "usuario@ejemplo.com",
        "roles": ["Administrador", "Supervisor"],
        "status": "Active"
      },
      {
        "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
        "username": "usuario2",
        "fullName": "Usuario Ejemplo 2",
        "email": "usuario2@ejemplo.com",
        "roles": ["Administrador"],
        "status": "Active"
      }
    ]
  }
]
```

**Códigos de respuesta:**
- `200 OK`: Solicitud exitosa
- `500 Internal Server Error`: Error del servidor

## Endpoints de Permisos por Ruta

### Verificar Permisos de Usuario para una Ruta

Este endpoint permite verificar qué permisos tiene un usuario para una ruta específica.

**Endpoint:** `GET /api/user-modules/user/{userId}/route-permissions`

**Parámetros:**
- `userId` (path): ID del usuario (GUID)
- `route` (query): Ruta a verificar (string)

**Respuesta:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "usuario1",
  "fullName": "Usuario Ejemplo",
  "route": "/admin/users",
  "module": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Administración",
    "baseRoute": "/admin"
  },
  "permissions": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "users.read",
      "description": "Ver usuarios",
      "type": "Read",
      "sourceRole": "Administrador",
      "sourceRoleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    },
    {
      "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
      "name": "users.write",
      "description": "Modificar usuarios",
      "type": "Write",
      "sourceRole": "Administrador",
      "sourceRoleId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
    }
  ],
  "hasAccess": true
}
```

**Códigos de respuesta:**
- `200 OK`: Solicitud exitosa
- `400 Bad Request`: Parámetros inválidos
- `404 Not Found`: Usuario o módulo no encontrado
- `500 Internal Server Error`: Error del servidor

## Endpoints de Gestión de Permisos de Roles

### Asignar Permisos a un Rol

Este endpoint permite asignar nuevos permisos a un rol existente.

**Endpoint:** `POST /api/role-permissions/roles/{roleId}/permissions`

**Parámetros:**
- `roleId` (path): ID del rol (GUID)
- Cuerpo de la solicitud:
  ```json
  {
    "permissionIds": [
      "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "4fa85f64-5717-4562-b3fc-2c963f66afa7"
    ]
  }
  ```

**Respuesta:**
```json
{
  "message": "Permisos asignados correctamente al rol"
}
```

**Códigos de respuesta:**
- `200 OK`: Solicitud exitosa
- `400 Bad Request`: Permisos inválidos
- `404 Not Found`: Rol no encontrado
- `500 Internal Server Error`: Error del servidor

### Eliminar Permiso de un Rol

Este endpoint permite eliminar un permiso específico de un rol.

**Endpoint:** `DELETE /api/role-permissions/roles/{roleId}/permissions/{permissionId}`

**Parámetros:**
- `roleId` (path): ID del rol (GUID)
- `permissionId` (path): ID del permiso (GUID)

**Respuesta:**
```json
{
  "message": "Permiso eliminado correctamente del rol"
}
```

**Códigos de respuesta:**
- `200 OK`: Solicitud exitosa
- `404 Not Found`: Rol, permiso o relación no encontrada
- `500 Internal Server Error`: Error del servidor

### Actualizar Todos los Permisos de un Rol

Este endpoint permite reemplazar todos los permisos actuales de un rol con una nueva lista de permisos.

**Endpoint:** `PUT /api/role-permissions/roles/{roleId}/permissions`

**Parámetros:**
- `roleId` (path): ID del rol (GUID)
- Cuerpo de la solicitud:
  ```json
  {
    "permissionIds": [
      "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "4fa85f64-5717-4562-b3fc-2c963f66afa7",
      "5fa85f64-5717-4562-b3fc-2c963f66afa8"
    ]
  }
  ```

**Respuesta:**
```json
{
  "message": "Permisos del rol actualizados correctamente"
}
```

**Códigos de respuesta:**
- `200 OK`: Solicitud exitosa
- `400 Bad Request`: Permisos inválidos
- `404 Not Found`: Rol no encontrado
- `500 Internal Server Error`: Error del servidor

## Modelos de Datos

### UserModuleResponse

```csharp
public class UserModuleResponse
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public UserStatus Status { get; set; }
    public UserType UserType { get; set; }
    public DateTime? LastLogin { get; set; }
    public List<UserRoleInfo> Roles { get; set; }
    public UserDetailInfo DetailInfo { get; set; }
}

public class UserRoleInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public class UserDetailInfo
{
    public string PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public bool TwoFactorEnabled { get; set; }
}
```

### ModuleUsersGroupResponse

```csharp
public class ModuleUsersGroupResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Route { get; set; }
    public string Icon { get; set; }
    public bool IsEnabled { get; set; }
    public int UserCount { get; set; }
    public List<ModuleUserInfo> Users { get; set; }
}

public class ModuleUserInfo
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public List<string> Roles { get; set; }
    public string Status { get; set; }
}
```

### UserRoutePermissionsResponse

```csharp
public class UserRoutePermissionsResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Route { get; set; }
    public ModuleInfo Module { get; set; }
    public List<RoutePermissionInfo> Permissions { get; set; }
    public bool HasAccess { get; set; }
}

public class ModuleInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string BaseRoute { get; set; }
}

public class RoutePermissionInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public PermissionType Type { get; set; }
    public string SourceRole { get; set; }
    public Guid SourceRoleId { get; set; }
}
```

### RolePermissionAssignmentRequest

```csharp
public class RolePermissionAssignmentRequest
{
    public List<Guid> PermissionIds { get; set; }
}
```

## Ejemplos de Uso

### Obtener usuarios del módulo de administración

```http
GET /api/user-modules/module/3fa85f64-5717-4562-b3fc-2c963f66afa6
```

### Verificar permisos de un usuario para la ruta de gestión de usuarios

```http
GET /api/user-modules/user/3fa85f64-5717-4562-b3fc-2c963f66afa6/route-permissions?route=/admin/users
```

### Asignar permisos de lectura y escritura al rol de administrador

```http
POST /api/role-permissions/roles/3fa85f64-5717-4562-b3fc-2c963f66afa6/permissions
Content-Type: application/json

{
  "permissionIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",  // users.read
    "4fa85f64-5717-4562-b3fc-2c963f66afa7"   // users.write
  ]
}
```

## Consideraciones de Seguridad

- Todos los endpoints están protegidos con autenticación.
- Se recomienda implementar autorización adicional para restringir quién puede asignar permisos a roles.
- Las operaciones de modificación de permisos deben ser auditadas.
- Es importante verificar que los usuarios que acceden a estos endpoints tengan los permisos adecuados para gestionar usuarios, roles y permisos.
