using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Repositories;
using AuthSystem.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthSystem.Infrastructure.Persistence
{
    /// <summary>
    /// Implementación de la unidad de trabajo
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed = false;

        // Repositorios
        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private IPermissionRepository _permissionRepository;
        private IModuleRepository _moduleRepository;
        private IOrganizationRepository _organizationRepository;
        private IBranchRepository _branchRepository;
        private IUserRoleRepository _userRoleRepository;
        private IUserBranchRepository _userBranchRepository;
        private IRolePermissionRepository _rolePermissionRepository;
        private IUserSessionRepository _userSessionRepository;
        private IAuditLogRepository _auditLogRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Repositorio de usuarios
        /// </summary>
        public IUserRepository Users => _userRepository ??= new UserRepository(_context);

        /// <summary>
        /// Repositorio de roles
        /// </summary>
        public IRoleRepository Roles => _roleRepository ??= new RoleRepository(_context);

        /// <summary>
        /// Repositorio de permisos
        /// </summary>
        public IPermissionRepository Permissions => _permissionRepository ??= new PermissionRepository(_context);

        /// <summary>
        /// Repositorio de módulos
        /// </summary>
        public IModuleRepository Modules => _moduleRepository ??= new ModuleRepository(_context);

        /// <summary>
        /// Repositorio de organizaciones
        /// </summary>
        public IOrganizationRepository Organizations => _organizationRepository ??= new OrganizationRepository(_context);

        /// <summary>
        /// Repositorio de sucursales
        /// </summary>
        public IBranchRepository Branches => _branchRepository ??= new BranchRepository(_context);

        /// <summary>
        /// Repositorio de relaciones usuario-rol
        /// </summary>
        public IUserRoleRepository UserRoles => _userRoleRepository ??= new UserRoleRepository(_context);

        /// <summary>
        /// Repositorio de relaciones usuario-sucursal
        /// </summary>
        public IUserBranchRepository UserBranches => _userBranchRepository ??= new UserBranchRepository(_context);

        /// <summary>
        /// Repositorio de relaciones rol-permiso
        /// </summary>
        public IRolePermissionRepository RolePermissions => _rolePermissionRepository ??= new RolePermissionRepository(_context);

        /// <summary>
        /// Repositorio de sesiones de usuario
        /// </summary>
        public IUserSessionRepository UserSessions => _userSessionRepository ??= new UserSessionRepository(_context);

        /// <summary>
        /// Repositorio de logs de auditoría
        /// </summary>
        public IAuditLogRepository AuditLogs => _auditLogRepository ??= new AuditLogRepository(_context);

        /// <summary>
        /// Guarda todos los cambios realizados en el contexto
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de entidades modificadas</returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Inicia una nueva transacción
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Ya existe una transacción activa");
            }

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        /// <summary>
        /// Confirma la transacción actual
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No hay una transacción activa para confirmar");
            }

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await _transaction.CommitAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Revierte la transacción actual
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No hay una transacción activa para revertir");
            }

            try
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        /// <summary>
        /// Libera los recursos utilizados
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Libera los recursos utilizados
        /// </summary>
        /// <param name="disposing">Indica si se están liberando recursos administrados</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
