using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Common;
using AuthSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Module = AuthSystem.Domain.Entities.Module;

namespace AuthSystem.Infrastructure.Persistence
{
    /// <summary>
    /// Contexto de base de datos de la aplicación
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Opciones de configuración</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Usuarios
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Roles
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Permisos
        /// </summary>
        public DbSet<Permission> Permissions { get; set; }

        /// <summary>
        /// Módulos
        /// </summary>
        public DbSet<Module> Modules { get; set; }

        /// <summary>
        /// Organizaciones
        /// </summary>
        public DbSet<Organization> Organizations { get; set; }

        /// <summary>
        /// Sucursales
        /// </summary>
        public DbSet<Branch> Branches { get; set; }

        /// <summary>
        /// Relaciones usuario-rol
        /// </summary>
        public DbSet<UserRole> UserRoles { get; set; }

        /// <summary>
        /// Relaciones usuario-sucursal
        /// </summary>
        public DbSet<UserBranch> UserBranches { get; set; }

        /// <summary>
        /// Relaciones rol-permiso
        /// </summary>
        public DbSet<RolePermission> RolePermissions { get; set; }

        /// <summary>
        /// Sesiones de usuario
        /// </summary>
        public DbSet<UserSession> UserSessions { get; set; }

        /// <summary>
        /// Logs de auditoría
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// Configuración del modelo
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar configuraciones de todas las entidades
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Configurar relaciones y restricciones
            ConfigureUserRelationships(modelBuilder);
            ConfigureRoleRelationships(modelBuilder);
            ConfigurePermissionRelationships(modelBuilder);
            ConfigureModuleRelationships(modelBuilder);
            ConfigureOrganizationRelationships(modelBuilder);
            ConfigureBranchRelationships(modelBuilder);
        }

        /// <summary>
        /// Configurar relaciones de la entidad Usuario
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo</param>
        private void ConfigureUserRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.UserBranches)
                .WithOne(ub => ub.User)
                .HasForeignKey(ub => ub.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserSession>()
                .HasOne(us => us.User)
                .WithMany()
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.LdapDN)
                .IsUnique()
                .HasFilter("[LdapDN] IS NOT NULL");
        }

        /// <summary>
        /// Configurar relaciones de la entidad Rol
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo</param>
        private void ConfigureRoleRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>()
                .HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Role>()
                .HasMany(r => r.RolePermissions)
                .WithOne(rp => rp.Role)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Role>()
                .HasOne(r => r.Organization)
                .WithMany(o => o.Roles)
                .HasForeignKey(r => r.OrganizationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Role>()
                .HasIndex(r => new { r.Name, r.OrganizationId })
                .IsUnique();
        }

        /// <summary>
        /// Configurar relaciones de la entidad Permiso
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo</param>
        private void ConfigurePermissionRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permission>()
                .HasMany(p => p.RolePermissions)
                .WithOne(rp => rp.Permission)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Permission>()
                .HasOne(p => p.Module)
                .WithMany(m => m.Permissions)
                .HasForeignKey(p => p.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Permission>()
                .HasIndex(p => new { p.Name, p.ModuleId })
                .IsUnique();
        }

        /// <summary>
        /// Configurar relaciones de la entidad Módulo
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo</param>
        private void ConfigureModuleRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Module>()
                .HasMany(m => m.Permissions)
                .WithOne(p => p.Module)
                .HasForeignKey(p => p.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Module>()
                .HasMany(m => m.Children)
                .WithOne(m => m.Parent)
                .HasForeignKey(m => m.ParentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Module>()
                .HasIndex(m => m.Name)
                .IsUnique();

            modelBuilder.Entity<Module>()
                .HasIndex(m => m.Route)
                .IsUnique();
        }

        /// <summary>
        /// Configurar relaciones de la entidad Organización
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo</param>
        private void ConfigureOrganizationRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Organization>()
                .HasMany(o => o.Branches)
                .WithOne(b => b.Organization)
                .HasForeignKey(b => b.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Organization>()
                .HasMany(o => o.Roles)
                .WithOne(r => r.Organization)
                .HasForeignKey(r => r.OrganizationId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Organization>()
                .HasIndex(o => o.Name)
                .IsUnique();

            modelBuilder.Entity<Organization>()
                .HasIndex(o => o.TaxId)
                .IsUnique();
        }

        /// <summary>
        /// Configurar relaciones de la entidad Sucursal
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo</param>
        private void ConfigureBranchRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Branch>()
                .HasMany(b => b.UserBranches)
                .WithOne(ub => ub.Branch)
                .HasForeignKey(ub => ub.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Branch>()
                .HasOne(b => b.Organization)
                .WithMany(o => o.Branches)
                .HasForeignKey(b => b.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Branch>()
                .HasIndex(b => new { b.Code, b.OrganizationId })
                .IsUnique();
        }

        /// <summary>
        /// Guardar cambios
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades modificadas</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Establecer propiedades de auditoría
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTime.UtcNow;
                        entry.Entity.IsActive = true;
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModifiedAt = DateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
