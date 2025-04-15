# Documentación de Repositorios - AuthSystem

## Descripción General

Este documento describe la implementación de los repositorios en la capa de infraestructura del sistema de autenticación y autorización. Los repositorios siguen el patrón de repositorio genérico y proporcionan métodos específicos para cada entidad del dominio.

## Repositorio Genérico

El repositorio genérico (`Repository<T>`) implementa la interfaz `IRepository<T>` y proporciona operaciones CRUD básicas para todas las entidades. Características principales:

- Implementación de operaciones CRUD (Create, Read, Update, Delete)
- Soporte para operaciones asíncronas
- Manejo de entidades activas/inactivas (soft delete)
- Paginación para consultas grandes

## Repositorios Específicos

### AuditLogRepository

Implementación para el registro de auditoría con métodos para buscar logs por diversos criterios.

**Métodos principales:**
- `GetByUserAsync`: Obtiene logs de auditoría por usuario
- `GetByActionTypeAsync`: Obtiene logs por tipo de acción
- `GetByEntityAsync`: Obtiene logs por entidad
- `GetByOrganizationAsync`: Obtiene logs por organización
- `GetByBranchAsync`: Obtiene logs por sucursal
- `GetByDateRangeAsync`: Obtiene logs por rango de fechas
- `GetByModuleAsync`: Obtiene logs por módulo
- `GetBySeverityAsync`: Obtiene logs por nivel de severidad
- `GetFailedActionsAsync`: Obtiene logs de acciones fallidas
- `SearchAsync`: Búsqueda avanzada con múltiples filtros y paginación

### ModuleRepository

Gestiona los módulos del sistema, permitiendo obtener y organizar la estructura jerárquica de módulos.

**Métodos principales:**
- `GetByNameAsync`: Obtiene un módulo por su nombre
- `GetByRouteAsync`: Obtiene un módulo por su ruta
- `GetMainModulesAsync`: Obtiene los módulos principales (sin módulo padre)
- `GetSubmodulesAsync`: Obtiene los módulos hijos de un módulo
- `GetModuleTreeAsync`: Obtiene el árbol completo de módulos
- `SearchAsync`: Búsqueda avanzada con múltiples filtros
- `GetAccessibleModulesForUserAsync`: Obtiene los módulos accesibles para un usuario
- `UserHasModuleAccessAsync`: Verifica si un usuario tiene acceso a un módulo

**Relaciones importantes:**
- `Parent`: Referencia al módulo padre
- `Children`: Colección de módulos hijos
- `ParentId`: ID del módulo padre

### OrganizationRepository

Maneja las organizaciones con métodos para buscar y configurar conexiones LDAP.

**Métodos principales:**
- `GetByNameAsync`: Obtiene una organización por su nombre
- `GetByTaxIdAsync`: Obtiene una organización por su identificación fiscal
- `GetByUserAsync`: Obtiene organizaciones por usuario (a través de las sucursales del usuario)
- `SearchAsync`: Búsqueda avanzada con múltiples filtros
- `ConfigureLdapConnectionAsync`: Configura la conexión LDAP para una organización

**Propiedades importantes:**
- `LdapConfig`: Configuración LDAP de la organización (en lugar de propiedades individuales)

### BranchRepository

Administra las sucursales de las organizaciones.

**Métodos principales:**
- `GetByCodeAsync`: Obtiene una sucursal por su código
- `GetByCodeAndOrganizationAsync`: Obtiene una sucursal por su código y organización
- `GetByOrganizationAsync`: Obtiene sucursales por organización
- `GetByUserAsync`: Obtiene sucursales por usuario
- `SearchAsync`: Búsqueda avanzada con múltiples filtros

### UserRepository

Gestiona los usuarios del sistema.

**Métodos principales:**
- `GetByUsernameAsync`: Obtiene un usuario por su nombre de usuario
- `GetByEmailAsync`: Obtiene un usuario por su correo electrónico
- `GetByExternalIdAsync`: Obtiene un usuario por su ID externo (utilizando LdapDN)
- `GetByLdapDNAsync`: Obtiene un usuario por su DN de LDAP
- `GetByOrganizationAsync`: Obtiene usuarios por organización (a través de sus sucursales)
- `GetByRoleAsync`: Obtiene usuarios por rol
- `SearchAsync`: Búsqueda avanzada con múltiples filtros

**Propiedades importantes:**
- `FullName`: Nombre completo del usuario (en lugar de FirstName y LastName separados)
- `LdapDN`: Identificador LDAP del usuario (usado como ID externo)

### RoleRepository

Gestiona los roles del sistema.

**Métodos principales:**
- `GetByNameAsync`: Obtiene un rol por su nombre
- `GetByNameAndOrganizationAsync`: Obtiene un rol por su nombre y organización
- `GetByOrganizationAsync`: Obtiene roles por organización
- `GetByUserAsync`: Obtiene roles por usuario
- `SearchAsync`: Búsqueda avanzada con múltiples filtros

### PermissionRepository

Gestiona los permisos del sistema.

**Métodos principales:**
- `GetByNameAsync`: Obtiene un permiso por su nombre
- `GetByCodeAsync`: Obtiene un permiso por su código (utilizando Name como identificador)
- `GetByModuleAsync`: Obtiene permisos por módulo
- `GetByRoleAsync`: Obtiene permisos por rol
- `SearchAsync`: Búsqueda avanzada con múltiples filtros

**Propiedades importantes:**
- `Name`: Nombre del permiso (usado como identificador principal)

### UserRoleRepository

Gestiona las relaciones entre usuarios y roles.

**Métodos principales:**
- `GetByUserAsync`: Obtiene relaciones usuario-rol por usuario
- `GetByRoleAsync`: Obtiene relaciones usuario-rol por rol
- `GetByUserAndRoleAsync`: Obtiene una relación usuario-rol específica
- `AssignRolesToUserAsync`: Asigna múltiples roles a un usuario
- `RemoveAllRolesFromUserAsync`: Elimina todos los roles de un usuario
- `RemoveRolesFromUserAsync`: Elimina roles específicos de un usuario

### UserBranchRepository

Administra las relaciones entre usuarios y sucursales.

**Métodos principales:**
- `GetByUserAsync`: Obtiene relaciones usuario-sucursal por usuario
- `GetByBranchAsync`: Obtiene relaciones usuario-sucursal por sucursal
- `GetByOrganizationAsync`: Obtiene relaciones usuario-sucursal por organización
- `GetByUserAndBranchAsync`: Obtiene una relación usuario-sucursal específica
- `AssignBranchesToUserAsync`: Asigna múltiples sucursales a un usuario
- `RemoveAllBranchesFromUserAsync`: Elimina todas las sucursales de un usuario
- `RemoveBranchesFromUserAsync`: Elimina sucursales específicas de un usuario

### RolePermissionRepository

Gestiona las relaciones entre roles y permisos.

**Métodos principales:**
- `GetByRoleAsync`: Obtiene relaciones rol-permiso por rol
- `GetByPermissionAsync`: Obtiene relaciones rol-permiso por permiso
- `GetByRoleAndPermissionAsync`: Obtiene una relación rol-permiso específica
- `AssignPermissionsToRoleAsync`: Asigna múltiples permisos a un rol
- `RemoveAllPermissionsFromRoleAsync`: Elimina todos los permisos de un rol
- `RemovePermissionsFromRoleAsync`: Elimina permisos específicos de un rol

### UserSessionRepository

Gestiona las sesiones de usuario.

**Métodos principales:**
- `GetByUserAsync`: Obtiene sesiones por usuario
- `GetByTokenAsync`: Obtiene una sesión por su token
- `GetActiveSessionsAsync`: Obtiene todas las sesiones activas
- `RevokeSessionAsync`: Revoca una sesión específica
- `RevokeAllUserSessionsAsync`: Revoca todas las sesiones de un usuario

## Unidad de Trabajo (UnitOfWork)

La clase `UnitOfWork` implementa la interfaz `IUnitOfWork` y coordina transacciones a través de múltiples repositorios.

**Características principales:**
- Gestión de transacciones
- Commit y rollback de cambios
- Acceso centralizado a todos los repositorios

## Registro de Dependencias

La clase `DependencyInjection` proporciona métodos de extensión para registrar todos los repositorios y servicios en el contenedor de dependencias de la aplicación.

**Configuraciones principales:**
- Registro del contexto de base de datos con Entity Framework Core
- Registro de todos los repositorios específicos
- Registro de la unidad de trabajo

## Consideraciones de Implementación

- **Soft Delete**: Todas las entidades implementan borrado lógico mediante la propiedad `IsActive`
- **Auditoría**: Las entidades utilizan `CreatedAt`, `CreatedBy`, `LastModifiedAt` y `LastModifiedBy` para seguimiento de cambios
- **Validaciones**: Se incluyen validaciones de parámetros en todos los métodos
- **Relaciones**: Se cargan las relaciones necesarias mediante `Include` y `ThenInclude`
- **Paginación**: Los métodos de búsqueda incluyen paginación para optimizar el rendimiento
- **Asincronía**: Todos los métodos son asincrónicos para mejorar la escalabilidad
