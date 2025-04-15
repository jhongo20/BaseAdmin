using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de sucursales
    /// </summary>
    public interface IBranchRepository : IRepository<Branch>
    {
        /// <summary>
        /// Obtiene una sucursal por su código
        /// </summary>
        /// <param name="code">Código de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sucursal encontrada o null</returns>
        Task<Branch> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene sucursales por organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sucursales de la organización</returns>
        Task<IReadOnlyList<Branch>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene la sede principal de una organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sede principal de la organización</returns>
        Task<Branch> GetHeadquartersAsync(Guid organizationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene sucursales asignadas a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sucursales asignadas al usuario</returns>
        Task<IReadOnlyList<Branch>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene la sucursal principal de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sucursal principal del usuario</returns>
        Task<Branch> GetPrimaryBranchForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
