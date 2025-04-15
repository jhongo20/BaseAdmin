# Correcciones de Repositorios - Detalles Técnicos

## Descripción General

Este documento proporciona detalles técnicos específicos sobre las correcciones realizadas en cada repositorio para resolver los errores de compilación en el sistema AuthSystem.

## ApplicationDbContext

### Correcciones en Configuraciones de Relaciones

#### Configuración de Módulos
```csharp
// Antes
modelBuilder.Entity<Module>()
    .HasMany(m => m.ChildModules)
    .WithOne(m => m.ParentModule)
    .HasForeignKey(m => m.ParentModuleId)
    .IsRequired(false)
    .OnDelete(DeleteBehavior.Restrict);

// Después
modelBuilder.Entity<Module>()
    .HasMany(m => m.Children)
    .WithOne(m => m.Parent)
    .HasForeignKey(m => m.ParentId)
    .IsRequired(false)
    .OnDelete(DeleteBehavior.Restrict);
```

#### Configuración de Usuarios
```csharp
// Antes
modelBuilder.Entity<User>()
    .HasIndex(u => u.ExternalId)
    .IsUnique()
    .HasFilter("[ExternalId] IS NOT NULL");

// Después
modelBuilder.Entity<User>()
    .HasIndex(u => u.LdapDN)
    .IsUnique()
    .HasFilter("[LdapDN] IS NOT NULL");
```

#### Configuración de Sesiones de Usuario
```csharp
// Antes
modelBuilder.Entity<User>()
    .HasMany(u => u.UserSessions)
    .WithOne(us => us.User)
    .HasForeignKey(us => us.UserId)
    .OnDelete(DeleteBehavior.Cascade);

// Después
modelBuilder.Entity<UserSession>()
    .HasOne(us => us.User)
    .WithMany()
    .HasForeignKey(us => us.UserId)
    .OnDelete(DeleteBehavior.Cascade);
```

#### Eliminación de Relación Incorrecta
```csharp
// Eliminado
modelBuilder.Entity<User>()
    .HasOne(u => u.Organization)
    .WithMany(o => o.Users)
    .HasForeignKey(u => u.OrganizationId)
    .IsRequired(false)
    .OnDelete(DeleteBehavior.SetNull);

// Eliminado
modelBuilder.Entity<Organization>()
    .HasMany(o => o.Users)
    .WithOne(u => u.Organization)
    .HasForeignKey(u => u.OrganizationId)
    .IsRequired(false)
    .OnDelete(DeleteBehavior.SetNull);
```

## RolePermissionRepository

### Correcciones en Propiedades de Auditoría

```csharp
// Antes
rolePermission.ModifiedBy = removedBy;
rolePermission.ModifiedAt = DateTime.UtcNow;

// Después
rolePermission.LastModifiedBy = removedBy.ToString();
rolePermission.LastModifiedAt = DateTime.UtcNow;
```

### Correcciones en Creación de Entidades

```csharp
// Antes
var newRolePermission = new RolePermission
{
    RoleId = roleId,
    PermissionId = permissionId,
    CreatedBy = assignedBy,
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};

// Después
var rolePermission = new RolePermission
{
    RoleId = roleId,
    PermissionId = permissionId,
    CreatedBy = assignedBy.ToString(),
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};
```

## UserRoleRepository

### Correcciones en Propiedades de Auditoría

```csharp
// Antes
existingUserRole.ModifiedBy = assignedBy;
existingUserRole.ModifiedAt = DateTime.UtcNow;

// Después
existingUserRole.LastModifiedBy = assignedBy.ToString();
existingUserRole.LastModifiedAt = DateTime.UtcNow;
```

### Correcciones en Creación de Entidades

```csharp
// Antes
var userRole = new UserRole
{
    UserId = userId,
    RoleId = roleId,
    CreatedBy = assignedBy,
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};

// Después
var userRole = new UserRole
{
    UserId = userId,
    RoleId = roleId,
    CreatedBy = assignedBy.ToString(),
    CreatedAt = DateTime.UtcNow,
    IsActive = true
};
```

## UserRepository

### Correcciones en Búsqueda por ID Externo

```csharp
// Antes
return await _dbSet
    .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
    .Include(u => u.UserBranches)
        .ThenInclude(ub => ub.Branch)
    .FirstOrDefaultAsync(u => u.ExternalId == externalId && u.IsActive, cancellationToken);

// Después
return await _dbSet
    .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
    .Include(u => u.UserBranches)
        .ThenInclude(ub => ub.Branch)
    .FirstOrDefaultAsync(u => u.LdapDN == externalId && u.IsActive, cancellationToken);
```

### Correcciones en Búsqueda por Organización

```csharp
// Antes
return await _dbSet
    .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
    .Where(u => u.OrganizationId == organizationId && u.IsActive)
    .OrderBy(u => u.LastName)
    .ThenBy(u => u.FirstName)
    .ToListAsync(cancellationToken);

// Después
return await _dbSet
    .Include(u => u.UserRoles)
        .ThenInclude(ur => ur.Role)
    .Include(u => u.UserBranches)
        .ThenInclude(ub => ub.Branch)
    .Where(u => u.UserBranches.Any(ub => ub.Branch.OrganizationId == organizationId) && u.IsActive)
    .OrderBy(u => u.FullName)
    .ToListAsync(cancellationToken);
```

### Correcciones en Criterios de Ordenamiento

```csharp
// Antes
.OrderBy(u => u.LastName)
.ThenBy(u => u.FirstName)

// Después
.OrderBy(u => u.FullName)
```

## OrganizationRepository

### Correcciones en Búsqueda por Usuario

```csharp
// Antes
// Si el usuario tiene una organización asignada directamente
if (user.OrganizationId.HasValue)
{
    var organization = await _dbSet
        .Include(o => o.Branches)
        .Include(o => o.Roles)
        .FirstOrDefaultAsync(o => o.Id == user.OrganizationId.Value && o.IsActive, cancellationToken);

    return organization != null ? new List<Organization> { organization } : new List<Organization>();
}

// Después
// El usuario se relaciona con organizaciones a través de sucursales
var userBranches = await _context.UserBranches
    .Include(ub => ub.Branch)
    .Where(ub => ub.UserId == userId && ub.IsActive)
    .ToListAsync(cancellationToken);

if (!userBranches.Any())
{
    return new List<Organization>();
}

// Obtener las organizaciones de las sucursales del usuario
var organizationIds = userBranches
    .Select(ub => ub.Branch.OrganizationId)
    .Distinct()
    .ToList();
```

### Correcciones en Configuración LDAP

```csharp
// Antes (referencia a propiedades individuales que no existen)
organization.LdapServer = ldapServer;
organization.LdapPort = ldapPort;
organization.LdapBaseDn = ldapBaseDn;
// ...

// Después (uso de una única propiedad LdapConfig)
organization.LdapConfig = new LdapConfig
{
    Server = ldapServer,
    Port = ldapPort,
    BaseDn = ldapBaseDn,
    // ...
};
```

## UserBranchRepository

### Correcciones en Criterios de Ordenamiento

```csharp
// Antes
.OrderBy(ub => ub.User.LastName)
.ThenBy(ub => ub.User.FirstName)

// Después
.OrderBy(ub => ub.User.FullName)
```

## UserSessionRepository

### Correcciones en Revocación de Sesiones

```csharp
// Antes
session.IsRevoked = true;
session.RevokedAt = now;
session.RevocationReason = reason;

// Después
session.IsRevoked = true;
session.LastModifiedAt = now;
session.RevocationReason = reason;
```

## PermissionRepository

### Correcciones en Búsqueda por Código

```csharp
// Antes
return await _dbSet
    .Include(p => p.Module)
    .Include(p => p.RolePermissions)
        .ThenInclude(rp => rp.Role)
    .FirstOrDefaultAsync(p => p.Code == code && p.IsActive, cancellationToken);

// Después
return await _dbSet
    .Include(p => p.Module)
    .Include(p => p.RolePermissions)
        .ThenInclude(rp => rp.Role)
    .FirstOrDefaultAsync(p => p.Name == code && p.IsActive, cancellationToken);
```

## Conclusión

Estas correcciones aseguran que los repositorios trabajen correctamente con la estructura real de las entidades, eliminando errores de compilación y mejorando la integridad del código. Es fundamental mantener la coherencia entre las entidades del dominio y sus repositorios correspondientes para garantizar el correcto funcionamiento del sistema.
