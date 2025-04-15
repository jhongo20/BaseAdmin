using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de organizaciones
    /// </summary>
    public interface IOrganizationRepository : IRepository<Organization>
    {
        /// <summary>
        /// Obtiene una organización por su nombre
        /// </summary>
        /// <param name="name">Nombre de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización encontrada o null</returns>
        Task<Organization> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una organización por su identificador fiscal
        /// </summary>
        /// <param name="taxId">Identificador fiscal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización encontrada o null</returns>
        Task<Organization> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una organización con sus sucursales
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización con sus sucursales</returns>
        Task<Organization> GetWithBranchesAsync(Guid organizationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una organización con sus roles
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización con sus roles</returns>
        Task<Organization> GetWithRolesAsync(Guid organizationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene organizaciones a las que pertenece un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de organizaciones a las que pertenece el usuario</returns>
        Task<IReadOnlyList<Organization>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
