# Documentación de Interfaces del Sistema

Este documento describe las interfaces principales definidas en la capa de dominio del sistema de autenticación y autorización.

## Interfaces de Repositorios

### IRepository<T>

Repositorio genérico para operaciones CRUD básicas.

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
```

### IUserRepository

Repositorio específico para la entidad User con métodos adicionales.

```csharp
public interface IUserRepository : IRepository<User>
{
    Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### IRoleRepository

Repositorio específico para la entidad Role.

```csharp
public interface IRoleRepository : IRepository<Role>
{
    Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Role>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### IRolePermissionRepository

Repositorio para gestionar las relaciones entre roles y permisos.

```csharp
public interface IRolePermissionRepository : IRepository<RolePermission>
{
    Task<IReadOnlyList<RolePermission>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RolePermission>> GetByPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default);
    Task<RolePermission> GetByRoleAndPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### IUserSessionRepository

Repositorio para gestionar las sesiones de usuario.

```csharp
public interface IUserSessionRepository : IRepository<UserSession>
{
    Task<IReadOnlyList<UserSession>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSession>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserSession> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### IAuditLogRepository

Repositorio para gestionar los registros de auditoría.

```csharp
public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByActionTypeAsync(AuditActionType actionType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(string entityName, string entityId = null, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### IUnitOfWork

Unidad de trabajo para coordinar transacciones entre múltiples repositorios.

```csharp
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IPermissionRepository Permissions { get; }
    // Otros repositorios...
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

## Interfaces de Servicios del Dominio

### IAuthService

Servicio para autenticación de usuarios (local y LDAP).

```csharp
public interface IAuthService
{
    Task<UserSession> AuthenticateLocalAsync(string username, string password, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<UserSession> AuthenticateLdapAsync(string username, string password, Guid organizationId, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<UserSession> RefreshTokenAsync(string refreshToken, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### IUserService

Servicio para gestión de usuarios.

```csharp
public interface IUserService
{
    Task<User> CreateExternalUserAsync(User user, string password, bool sendActivationEmail = true, CancellationToken cancellationToken = default);
    Task<User> CreateInternalUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### IRoleService

Servicio para gestión de roles.

```csharp
public interface IRoleService
{
    Task<Role> CreateRoleAsync(Role role, IEnumerable<Guid> permissionIds = null, CancellationToken cancellationToken = default);
    Task<Role> UpdateRoleAsync(Role role, CancellationToken cancellationToken = default);
    Task<bool> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### IPermissionService

Servicio para gestión de permisos.

```csharp
public interface IPermissionService
{
    Task<Permission> CreatePermissionAsync(Permission permission, Guid moduleId, CancellationToken cancellationToken = default);
    Task<Permission> UpdatePermissionAsync(Permission permission, CancellationToken cancellationToken = default);
    Task<bool> DeletePermissionAsync(Guid permissionId, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### IModuleService

Servicio para gestión de módulos funcionales.

```csharp
public interface IModuleService
{
    Task<Module> CreateModuleAsync(Module module, Guid? parentModuleId = null, bool createStandardPermissions = true, CancellationToken cancellationToken = default);
    Task<Module> UpdateModuleAsync(Module module, CancellationToken cancellationToken = default);
    Task<bool> DeleteModuleAsync(Guid moduleId, bool deleteChildModules = false, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### IOrganizationService

Servicio para gestión de organizaciones y sucursales.

```csharp
public interface IOrganizationService
{
    Task<Organization> CreateOrganizationAsync(Organization organization, bool createDefaultRoles = true, CancellationToken cancellationToken = default);
    Task<Organization> UpdateOrganizationAsync(Organization organization, CancellationToken cancellationToken = default);
    Task<bool> DeleteOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### IAuditService

Servicio para registro de acciones de auditoría.

```csharp
public interface IAuditService
{
    Task<AuditLog> LogActionAsync(Guid userId, AuditActionType actionType, string entityName, string entityId = null, string description = null, string oldValues = null, string newValues = null, string ipAddress = null, string userAgent = null, string moduleName = null, Guid? organizationId = null, Guid? branchId = null, string severity = "Information", bool isSuccessful = true, string errorMessage = null, CancellationToken cancellationToken = default);
    Task<AuditLog> LogLoginAttemptAsync(string username, bool isSuccessful, Guid? userId = null, string ipAddress = null, string userAgent = null, string errorMessage = null, Guid? organizationId = null, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

## Interfaces de Servicios de Infraestructura

### IEmailService

Servicio para envío de correos electrónicos.

```csharp
public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true, IEnumerable<string> cc = null, IEnumerable<string> bcc = null, IEnumerable<EmailAttachment> attachments = null, CancellationToken cancellationToken = default);
    Task<bool> SendWelcomeEmailAsync(string email, string username, string fullName, string activationToken = null, string activationUrl = null, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### ICacheService

Servicio para gestión de caché (Redis).

```csharp
public interface ICacheService
{
    Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### ILoggerService

Servicio para registro de logs.

```csharp
public interface ILoggerService
{
    void Information(string message, params object[] args);
    void Warning(string message, params object[] args);
    void Error(string message, params object[] args);
    void Error(Exception exception, string message, params object[] args);
    // Métodos adicionales...
}
```

### ILdapService

Servicio para integración con LDAP/Active Directory.

```csharp
public interface ILdapService
{
    Task<bool> AuthenticateAsync(string username, string password, Guid organizationId, CancellationToken cancellationToken = default);
    Task<LdapUserInfo> GetUserInfoAsync(string username, Guid organizationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LdapUserInfo>> SearchUsersAsync(string searchQuery, Guid organizationId, int maxResults = 100, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```

### IJwtService

Servicio para gestión de tokens JWT.

```csharp
public interface IJwtService
{
    Task<string> GenerateTokenAsync(Guid userId, string username, string email, IEnumerable<string> roles, IEnumerable<string> permissions, Guid? organizationId = null, IEnumerable<Guid> branchIds = null, IDictionary<string, string> additionalClaims = null, int expirationMinutes = 60, CancellationToken cancellationToken = default);
    Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, bool validateLifetime = true, CancellationToken cancellationToken = default);
    // Métodos adicionales...
}
```
