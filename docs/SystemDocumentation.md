# Documentación del Sistema de Autenticación (AuthSystem)

## Introducción

AuthSystem es un sistema completo de autenticación y autorización desarrollado con ASP.NET Core 8.0, siguiendo los principios de Clean Architecture. El sistema proporciona una solución robusta para la gestión de usuarios, roles, permisos y organizaciones, con características avanzadas de seguridad.

## Arquitectura

El sistema sigue una arquitectura de capas basada en Clean Architecture:

1. **Capa de Dominio (AuthSystem.Domain)**
   - Entidades del negocio
   - Interfaces de repositorios y servicios
   - Objetos de valor y enumeraciones

2. **Capa de Aplicación (AuthSystem.Application)**
   - Lógica de aplicación
   - DTOs (Data Transfer Objects)
   - Implementación de casos de uso
   - Validaciones

3. **Capa de Infraestructura (AuthSystem.Infrastructure)**
   - Implementación de repositorios
   - Acceso a datos (Entity Framework Core)
   - Servicios externos (Email, LDAP, etc.)
   - Implementaciones de seguridad

4. **Capa de API (AuthSystem.API)**
   - Controladores REST
   - Middlewares
   - Configuración de la aplicación
   - Documentación Swagger

## Arquitectura del Sistema

El sistema está construido siguiendo una arquitectura limpia (Clean Architecture) con separación clara de responsabilidades:

- **Capa de Presentación**: API REST con controladores y middleware.
- **Capa de Aplicación**: Servicios de aplicación y lógica de negocio.
- **Capa de Dominio**: Entidades, interfaces y reglas de negocio.
- **Capa de Infraestructura**: Implementaciones concretas de repositorios, servicios externos, etc.

### API Versionada

El sistema implementa una estrategia de versionado de API basada en la URL, permitiendo mantener múltiples versiones simultáneamente sin romper la compatibilidad con clientes existentes. Para más detalles, consulte la [documentación de API versionada](VersionedAPI.md).

### Gestión de Sesiones Distribuidas

El sistema utiliza un enfoque de sesiones distribuidas que permite gestionar las sesiones de usuario de manera centralizada y escalable, facilitando el cierre forzado de sesiones, la limitación de sesiones concurrentes y el monitoreo en tiempo real. Para más detalles, consulte la [documentación de sesiones distribuidas](DistributedSessions.md).

### Revocación de Tokens JWT

El sistema implementa un mecanismo robusto para la revocación de tokens JWT antes de su expiración natural, permitiendo invalidar sesiones en escenarios como cambios de contraseña, detección de actividad sospechosa o cierre de sesión manual. Para más detalles, consulte la [documentación de revocación de tokens](TokenRevocation.md).

### Feature Flags

El sistema incluye un mecanismo de Feature Flags (banderas de características) que permite activar o desactivar funcionalidades específicas sin necesidad de recompilar o redesplegar la aplicación. Esto facilita las pruebas A/B, lanzamientos graduales y el control de acceso a funcionalidades experimentales. Para más detalles, consulte la [documentación de Feature Flags](FeatureFlags.md).

### Logging Estructurado

El sistema implementa un sistema de logging estructurado con Serilog que permite registrar eventos y errores de manera eficiente y organizada. Los logs se almacenan en formato JSON y texto plano, se rotan diariamente y se enriquecen con información contextual como el usuario, la solicitud HTTP y el entorno. Para más detalles, consulte la [documentación de Logging](Logging.md).

### Sistema de Caché

El sistema implementa un mecanismo de caché que permite almacenar resultados de consultas frecuentes, reduciendo la carga en servicios externos y bases de datos, y mejorando significativamente el rendimiento de la aplicación. Soporta múltiples proveedores (Redis y memoria) y está especialmente optimizado para almacenar resultados de consultas LDAP y configuración. Para más detalles, consulte la [documentación de Caché](Caching.md).

## Entidades principales

El sistema gestiona las siguientes entidades principales:

- **User**: Usuarios del sistema (locales o LDAP)
- **Role**: Roles que definen conjuntos de permisos
- **Permission**: Permisos individuales asociados a módulos
- **Module**: Módulos o secciones del sistema
- **Organization**: Organizaciones que agrupan usuarios
- **Branch**: Sucursales pertenecientes a organizaciones
- **AuditLog**: Registro de acciones realizadas en el sistema
- **UserSession**: Sesiones de usuario activas y su información
- **RevokedToken**: Tokens JWT que han sido revocados manualmente

## Características principales

### 1. Autenticación

- Autenticación local con nombre de usuario y contraseña
- Integración con LDAP/Active Directory
- Tokens JWT para autenticación stateless
- Tokens de refresco para renovación de sesiones
- Bloqueo de cuentas tras múltiples intentos fallidos de inicio de sesión

### 2. Autorización

- Sistema basado en roles y permisos
- Permisos granulares por módulo
- Roles a nivel de sistema y organización
- Validación de permisos en endpoints

### 3. Gestión de usuarios

- Registro de usuarios
- Activación de cuentas por email
- Restablecimiento de contraseñas
- Perfiles de usuario
- Bloqueo/desbloqueo de usuarios

### 4. Gestión de organizaciones

- Creación y administración de organizaciones
- Sucursales por organización
- Asignación de usuarios a organizaciones y sucursales

### 5. Auditoría

- Registro detallado de acciones
- Seguimiento de cambios en entidades
- Consulta de logs de auditoría

### 6. Seguridad

- Encriptación de contraseñas con BCrypt
- Protección contra ataques CSRF
- Rate limiting (limitación de solicitudes)
- Headers de seguridad HTTP
- Validación de entradas
- Revocación de tokens JWT
- Gestión centralizada de sesiones
- Límite configurable de sesiones concurrentes

## Base de datos

El sistema utiliza Entity Framework Core como ORM (Object-Relational Mapper) con SQL Server como motor de base de datos. La estructura de la base de datos incluye:

1. **Tablas principales**:
   - Users, Roles, Permissions, Modules, Organizations, Branches
   
2. **Tablas de relación**:
   - UserRoles, RolePermissions, UserBranches
   
3. **Tablas de sistema**:
   - AuditLogs, UserSessions, RevokedTokens

## Rate Limiting

El sistema implementa una protección robusta contra abusos mediante rate limiting. Para más detalles, consulte la [documentación específica de Rate Limiting](./RateLimit.md).

## API REST

La API REST proporciona endpoints para todas las funcionalidades del sistema:

### Autenticación
- POST /api/auth/login
- POST /api/auth/refresh-token
- POST /api/auth/logout

### Revocación de Tokens
- POST /api/token-revocation/current
- POST /api/token-revocation/all
- POST /api/token-revocation/user/{userId}
- DELETE /api/token-revocation/cleanup

### Gestión de Sesiones
- GET /api/sessions/my
- DELETE /api/sessions/my/{sessionId}
- DELETE /api/sessions/my
- GET /api/sessions
- DELETE /api/sessions/{sessionId}
- DELETE /api/sessions/user/{userId}
- GET /api/sessions/stats
- POST /api/sessions/cleanup

### Feature Flags
- GET /api/v1/feature-flags
- GET /api/v1/feature-flags/{featureName}
- PUT /api/v1/feature-flags/{featureName}
- POST /api/v1/feature-flags/reload

### Usuarios
- GET /api/users
- GET /api/users/{id}
- POST /api/users
- PUT /api/users/{id}
- DELETE /api/users/{id}

### Roles
- GET /api/roles
- GET /api/roles/{id}
- POST /api/roles
- PUT /api/roles/{id}
- DELETE /api/roles/{id}

### Permisos
- GET /api/permissions
- GET /api/permissions/{id}
- POST /api/permissions
- PUT /api/permissions/{id}
- DELETE /api/permissions/{id}

### Módulos
- GET /api/modules
- GET /api/modules/{id}
- POST /api/modules
- PUT /api/modules/{id}
- DELETE /api/modules/{id}

### Gestión de Usuarios por Módulo
- GET /api/user-modules/module/{moduleId} - Obtener usuarios por módulo
- GET /api/user-modules/grouped - Obtener usuarios agrupados por módulo
- GET /api/user-modules/user/{userId}/route-permissions - Verificar permisos de usuario para una ruta

### Gestión de Permisos de Roles
- POST /api/role-permissions/roles/{roleId}/permissions - Asignar permisos a un rol
- DELETE /api/role-permissions/roles/{roleId}/permissions/{permissionId} - Eliminar permiso de un rol
- PUT /api/role-permissions/roles/{roleId}/permissions - Actualizar todos los permisos de un rol

### Organizaciones
- GET /api/organizations
- GET /api/organizations/{id}
- POST /api/organizations
- PUT /api/organizations/{id}
- DELETE /api/organizations/{id}

### Sucursales
- GET /api/branches
- GET /api/branches/{id}
- POST /api/branches
- PUT /api/branches/{id}
- DELETE /api/branches/{id}

## Configuración

La configuración del sistema se encuentra en el archivo `appsettings.json` y se divide en las siguientes secciones:

1. **ConnectionStrings**: Cadenas de conexión a la base de datos
2. **JwtSettings**: Configuración de tokens JWT
3. **LdapSettings**: Configuración de conexión LDAP
4. **RateLimiting**: Configuración de limitación de solicitudes
5. **Redis**: Configuración de caché Redis (opcional)
6. **CacheSettings**: Configuración del sistema de caché
7. **EmailSettings**: Configuración del servidor SMTP
8. **Serilog**: Configuración de logging estructurado
9. **SessionManagement**: Configuración del sistema de sesiones distribuidas
10. **FeatureFlags**: Configuración de banderas de características

## Seguridad

### Autenticación

El sistema utiliza JWT (JSON Web Tokens) para la autenticación de usuarios. Los tokens son firmados con una clave secreta y tienen un tiempo de expiración configurable.

### Revocación de Tokens

El sistema incluye un mecanismo de revocación de tokens que permite invalidar tokens JWT antes de su fecha de expiración natural. Esto es útil en escenarios como cierre de sesión, cambio de contraseña, detección de actividad sospechosa, revocación de permisos o bloqueo de cuenta.

Los tokens revocados se almacenan en la base de datos y son verificados en cada solicitud mediante un middleware especializado. Para más detalles, consulte la [documentación de revocación de tokens](TokenRevocation.md).

### Protección contra ataques

El sistema implementa varias capas de protección:

1. **Rate Limiting**: Limita el número de solicitudes por IP y cliente
2. **Bloqueo de cuentas**: 
   - Bloqueo temporal después de múltiples intentos fallidos de inicio de sesión
   - Duración y número de intentos configurables
   - Desbloqueo manual por administradores
   - Registro detallado de intentos fallidos
3. **CAPTCHA**:
   - Se muestra después de cierto número de intentos fallidos
   - Generación dinámica de imágenes
   - Validación segura del lado del servidor
4. **Autenticación de dos factores (2FA)**:
   - Basada en algoritmo TOTP (Time-based One-Time Password)
   - Compatible con Google Authenticator, Microsoft Authenticator, etc.
   - Códigos de recuperación para acceso de emergencia
5. **Monitoreo de seguridad**:
   - Detección de patrones sospechosos
   - Alertas en tiempo real
   - Notificaciones al equipo de seguridad
   - Estadísticas y métricas de seguridad
6. **Headers de seguridad**:
   - Content-Security-Policy
   - X-XSS-Protection
   - X-Frame-Options
   - X-Content-Type-Options
7. **Validación de entradas**: Previene inyecciones SQL y XSS

## Documentación adicional

- [Arquitectura del sistema](./Arquitectura.md)
- [Implementación de seguridad](./ImplementacionSeguridad.md)
- [Limitación de tasa (Rate Limiting)](./RateLimit.md)
- [Bloqueo de cuentas](./AccountLockout.md)
- [Características de seguridad](./SecurityFeatures.md)
- [Gestión de usuarios por módulo](./UserModulePermissions.md)

## Datos iniciales

El sistema incluye datos iniciales (seed data) para su funcionamiento:

1. **Módulos predefinidos**:
   - Dashboard, Usuarios, Roles, Permisos, Organizaciones, Sucursales, Logs de Auditoría

2. **Permisos por módulo**:
   - Permisos CRUD para cada módulo

3. **Roles predefinidos**:
   - Administrador: Acceso completo
   - Usuario: Acceso limitado

4. **Usuario administrador**:
   - Username: admin
   - Email: admin@system.com
   - Contraseña: Admin123!

## Pruebas

El sistema incluye pruebas unitarias e integración:

1. **Pruebas unitarias**: Verifican componentes individuales
2. **Pruebas de integración**: Verifican la interacción entre componentes
3. **Pruebas de API**: Verifican los endpoints REST

## Despliegue

Requisitos para el despliegue:

1. **.NET 8.0 SDK**
2. **SQL Server** (2016 o superior)
3. **Redis** (opcional, para entornos distribuidos)

Pasos para el despliegue:

1. Clonar el repositorio
2. Configurar `appsettings.json`
3. Ejecutar migraciones de base de datos
4. Compilar y publicar la aplicación
5. Configurar el servidor web (IIS, Nginx, etc.)

## Mantenimiento

Tareas de mantenimiento recomendadas:

1. **Backups regulares** de la base de datos
2. **Monitoreo** de logs de auditoría y errores
3. **Actualización** de dependencias
4. **Revisión** de configuraciones de seguridad

## Extensibilidad

El sistema está diseñado para ser extensible:

1. **Nuevos módulos**: Añadir nuevas funcionalidades
2. **Integración con servicios externos**: Implementar nuevos proveedores
3. **Personalización de UI**: Desarrollar interfaces personalizadas

## Soporte

Para soporte técnico o consultas:

- Email: soporte@mintrabajo.gov.co
- Documentación: /docs
- Repositorio: [URL del repositorio]
