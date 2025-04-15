using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Implementación del repositorio de organizaciones
    /// </summary>
    public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Contexto de base de datos</param>
        public OrganizationRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene una organización por su nombre
        /// </summary>
        /// <param name="name">Nombre de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización encontrada o null</returns>
        public async Task<Organization> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("El nombre de la organización no puede ser nulo o vacío", nameof(name));
            }

            return await _dbSet
                .Include(o => o.Branches)
                .Include(o => o.Roles)
                .FirstOrDefaultAsync(o => o.Name == name && o.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene una organización por su identificación fiscal
        /// </summary>
        /// <param name="taxId">Identificación fiscal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización encontrada o null</returns>
        public async Task<Organization> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(taxId))
            {
                throw new ArgumentException("La identificación fiscal no puede ser nula o vacía", nameof(taxId));
            }

            return await _dbSet
                .Include(o => o.Branches)
                .Include(o => o.Roles)
                .FirstOrDefaultAsync(o => o.TaxId == taxId && o.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene una organización con sus sucursales
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización con sus sucursales</returns>
        public async Task<Organization> GetWithBranchesAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(o => o.Branches)
                .FirstOrDefaultAsync(o => o.Id == organizationId && o.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene una organización con sus roles
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización con sus roles</returns>
        public async Task<Organization> GetWithRolesAsync(Guid organizationId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(o => o.Roles)
                .FirstOrDefaultAsync(o => o.Id == organizationId && o.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene organizaciones por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de organizaciones del usuario</returns>
        public async Task<IReadOnlyList<Organization>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // Obtener el usuario con sus sucursales
            var user = await _context.Users
                .Include(u => u.UserBranches)
                    .ThenInclude(ub => ub.Branch)
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

            if (user == null)
            {
                return new List<Organization>();
            }

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

            if (organizationIds.Count == 0)
            {
                return new List<Organization>();
            }

            return await _dbSet
                .Include(o => o.Branches)
                .Include(o => o.Roles)
                .Where(o => organizationIds.Contains(o.Id) && o.IsActive)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Busca organizaciones con criterios avanzados
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (nombre, descripción, identificación fiscal)</param>
        /// <param name="isActive">Indica si está activa (opcional)</param>
        /// <param name="includeBranches">Indica si se deben incluir las sucursales</param>
        /// <param name="includeRoles">Indica si se deben incluir los roles</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de organizaciones que cumplen los criterios</returns>
        public async Task<(IReadOnlyList<Organization> Items, int TotalCount)> SearchAsync(
            string searchTerm = null,
            bool? isActive = null,
            bool includeBranches = false,
            bool includeRoles = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Organization> query = _dbSet;

            // Incluir relaciones según se solicite
            if (includeBranches)
            {
                query = query.Include(o => o.Branches);
            }

            if (includeRoles)
            {
                query = query.Include(o => o.Roles);
            }

            // Aplicar filtros
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(o =>
                    o.Name.ToLower().Contains(searchTerm) ||
                    (o.Description != null && o.Description.ToLower().Contains(searchTerm)) ||
                    o.TaxId.ToLower().Contains(searchTerm));
            }

            if (isActive.HasValue)
            {
                query = query.Where(o => o.IsActive == isActive.Value);
            }
            else
            {
                query = query.Where(o => o.IsActive);
            }

            // Contar total de resultados
            var totalCount = await query.CountAsync(cancellationToken);

            // Aplicar paginación
            var items = await query
                .OrderBy(o => o.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        /// <summary>
        /// Obtiene una organización por ID con todas sus relaciones
        /// </summary>
        /// <param name="id">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización encontrada o null</returns>
        public override async Task<Organization> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(o => o.Branches)
                .Include(o => o.Roles)
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive, cancellationToken);
        }

        /// <summary>
        /// Obtiene todas las organizaciones activas
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de organizaciones activas</returns>
        public override async Task<IReadOnlyList<Organization>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(o => o.Branches)
                .Where(o => o.IsActive)
                .OrderBy(o => o.Name)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Configura la conexión LDAP para una organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="ldapServer">Servidor LDAP</param>
        /// <param name="ldapPort">Puerto LDAP</param>
        /// <param name="ldapBaseDn">DN base LDAP</param>
        /// <param name="ldapUsername">Usuario LDAP para conexión</param>
        /// <param name="ldapPassword">Contraseña LDAP para conexión</param>
        /// <param name="ldapUseSSL">Indica si se debe usar SSL</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la configuración fue exitosa</returns>
        public async Task<bool> ConfigureLdapConnectionAsync(
            Guid organizationId,
            string ldapServer,
            int ldapPort,
            string ldapBaseDn,
            string ldapUsername,
            string ldapPassword,
            bool ldapUseSSL,
            CancellationToken cancellationToken = default)
        {
            var organization = await _dbSet.FindAsync(new object[] { organizationId }, cancellationToken);
            if (organization == null)
            {
                return false;
            }

            // Crear un objeto de configuración LDAP y serializarlo a JSON
            var ldapConfig = new
            {
                Server = ldapServer,
                Port = ldapPort,
                BaseDn = ldapBaseDn,
                Username = ldapUsername,
                Password = ldapPassword,
                UseSSL = ldapUseSSL,
                HasConfiguration = true
            };

            // Serializar la configuración a JSON y guardarla en la propiedad LdapConfig
            organization.LdapConfig = System.Text.Json.JsonSerializer.Serialize(ldapConfig);

            return true;
        }
    }
}
