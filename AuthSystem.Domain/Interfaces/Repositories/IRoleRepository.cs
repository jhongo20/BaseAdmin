using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de roles
    /// </summary>
    public interface IRoleRepository : IRepository<Role>
    {
        /// <summary>
        /// Obtiene un rol por su nombre
        /// </summary>
        /// <param name="name">Nombre del rol</param>
        /// <param name="organizationId">ID de la organización (opcional, para roles específicos de una organización)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Rol encontrado o null</returns>
        Task<Role> GetByNameAsync(string name, Guid? organizationId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene roles por organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles de la organización</returns>
        Task<IReadOnlyList<Role>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene roles con sus permisos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles con sus permisos</returns>
        Task<IReadOnlyList<Role>> GetWithPermissionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un rol con sus permisos
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Rol con sus permisos</returns>
        Task<Role> GetWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene roles asignados a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles asignados al usuario</returns>
        Task<IReadOnlyList<Role>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene roles asignados a un usuario en una organización específica
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de roles asignados al usuario en la organización</returns>
        Task<IReadOnlyList<Role>> GetByUserAndOrganizationAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default);
    }
}
