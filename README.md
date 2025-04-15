# AuthSystem - Sistema de Autenticación y Autorización

Sistema de autenticación y autorización desarrollado con .NET 8, siguiendo los principios de Clean Architecture. Este sistema está diseñado para soportar múltiples organizaciones, usuarios internos (LDAP/Active Directory) y externos (credenciales locales), con una gestión robusta de roles y permisos.

## Arquitectura

El proyecto sigue una arquitectura limpia (Clean Architecture) con las siguientes capas:

1. **Domain**: Contiene las entidades del negocio, interfaces y contratos.
   - Entidades principales: User, Role, Permission, Module, Organization, Branch, AuditLog, UserSession
   - Interfaces para repositorios y servicios

2. **Application**: Contiene la lógica de aplicación, DTOs, servicios y características.
   - Organizado por características (Features): Authentication, Organizations, Permissions, Roles, Users
   - Utiliza patrón CQRS (Commands/Queries)

3. **Infrastructure**: Implementa las interfaces del dominio y proporciona servicios técnicos.
   - Persistencia: ApplicationDbContext, UnitOfWork, Repositories
   - Autenticación: AuthService (LDAP, JWT)
   - Servicios: Email, Caching, Logging

4. **API**: Expone los servicios como API REST.
   - Controladores organizados por versiones (v1, v2)
   - Middleware para manejo de errores, autenticación, etc.

5. **Tests**: Pruebas unitarias e integración.

## Características Principales

### Seguridad y Autenticación
- Soporte para usuarios internos (LDAP/Active Directory) y externos (credenciales locales)
- Autenticación con JWT y validación robusta (firma, emisor, audiencia, expiración)
- Lista de revocación de tokens para gestionar tokens comprometidos
- Protección contra ataques de fuerza bruta y DDoS
- Hashing seguro de contraseñas con BCrypt
- Middleware para auditoría de acciones sensibles e intentos de inicio de sesión fallidos
- Cabeceras CSP para mitigar ataques XSS
- Validación extendida de JWT

### Gestión de Usuarios, Roles y Permisos
- Modelo multitenancy que soporta estructuras organizacionales
- Activación de cuentas mediante token o código de verificación enviado por correo
- Recuperación de contraseña mediante token seguro
- Gestión de sesiones distribuidas con capacidad de cierre de sesión forzado

### Estructura Organizacional
- Separación modular (gestión de usuarios, roles, permisos, módulos)
- Arquitectura de microservicios para autenticación, gestión de usuarios y autorización
- Endpoints de API versionados

### Infraestructura y Mejores Prácticas
- CQRS con MediatR para clara separación de comandos y consultas
- FluentValidation para validación de entrada
- Logging estructurado con Serilog o NLog
- Logging centralizado con Elastic/Kibana o Splunk
- Monitoreo con herramientas APM como Application Insights o New Relic
- Endpoints de health check para estado del servicio
- Redis para caché de datos frecuentemente accedidos, tokens y configuraciones
- Procesamiento de trabajos en segundo plano para tareas pesadas (envío de correos, generación de informes)
- Compresión Gzip/Brotli para respuestas HTTP

## Estructura de Interfaces

### Repositorios
- `IRepository<T>`: Repositorio genérico para operaciones CRUD
- `IUserRepository`: Repositorio para gestión de usuarios
- `IRoleRepository`: Repositorio para gestión de roles
- `IPermissionRepository`: Repositorio para gestión de permisos
- `IModuleRepository`: Repositorio para gestión de módulos
- `IOrganizationRepository`: Repositorio para gestión de organizaciones
- `IBranchRepository`: Repositorio para gestión de sucursales
- `IUserRoleRepository`: Repositorio para relaciones usuario-rol
- `IUserBranchRepository`: Repositorio para relaciones usuario-sucursal
- `IRolePermissionRepository`: Repositorio para relaciones rol-permiso
- `IUserSessionRepository`: Repositorio para sesiones de usuario
- `IAuditLogRepository`: Repositorio para logs de auditoría
- `IUnitOfWork`: Unidad de trabajo para coordinar transacciones

### Servicios del Dominio
- `IAuthService`: Servicio de autenticación
- `IUserService`: Servicio de gestión de usuarios
- `IRoleService`: Servicio de gestión de roles
- `IPermissionService`: Servicio de gestión de permisos
- `IModuleService`: Servicio de gestión de módulos
- `IOrganizationService`: Servicio de gestión de organizaciones y sucursales
- `IAuditService`: Servicio de auditoría

### Servicios de Infraestructura
- `IEmailService`: Servicio de correo electrónico
- `ICacheService`: Servicio de caché
- `ILoggerService`: Servicio de logging
- `ILdapService`: Servicio de integración con LDAP/Active Directory
- `IJwtService`: Servicio de gestión de tokens JWT

## Requisitos

- .NET 8.0 SDK
- SQL Server (o PostgreSQL)
- Redis (opcional, para caché)
- SMTP Server (para envío de correos)

## Configuración

La configuración del sistema se realiza a través de archivos appsettings.json y variables de entorno. Consulta la documentación detallada para más información sobre las opciones de configuración disponibles.

## Licencia

Este proyecto está licenciado bajo [MIT License](LICENSE).
