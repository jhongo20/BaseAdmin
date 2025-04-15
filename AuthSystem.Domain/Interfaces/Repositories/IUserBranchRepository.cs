using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de relaciones entre usuarios y sucursales
    /// </summary>
    public interface IUserBranchRepository : IRepository<UserBranch>
    {
        /// <summary>
        /// Obtiene relaciones de usuario-sucursal por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-sucursal</returns>
        Task<IReadOnlyList<UserBranch>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene relaciones de usuario-sucursal por sucursal
        /// </summary>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-sucursal</returns>
        Task<IReadOnlyList<UserBranch>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una relación usuario-sucursal específica
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación usuario-sucursal encontrada o null</returns>
        Task<UserBranch> GetByUserAndBranchAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene la sucursal principal de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación usuario-sucursal principal</returns>
        Task<UserBranch> GetPrimaryBranchForUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene relaciones de usuario-sucursal activas por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-sucursal activas</returns>
        Task<IReadOnlyList<UserBranch>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asigna múltiples sucursales a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="branchIds">Lista de IDs de sucursales</param>
        /// <param name="primaryBranchId">ID de la sucursal principal (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-sucursal creadas</returns>
        Task<IReadOnlyList<UserBranch>> AssignBranchesToUserAsync(
            Guid userId, 
            IEnumerable<Guid> branchIds, 
            Guid? primaryBranchId = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Establece una sucursal como la principal para un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Relación usuario-sucursal actualizada</returns>
        Task<UserBranch> SetPrimaryBranchForUserAsync(Guid userId, Guid branchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina todas las asignaciones de sucursales para un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task RemoveAllBranchesFromUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
