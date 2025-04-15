using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de módulos
    /// </summary>
    public interface IModuleRepository : IRepository<Module>
    {
        /// <summary>
        /// Obtiene un módulo por su nombre
        /// </summary>
        /// <param name="name">Nombre del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Módulo encontrado o null</returns>
        Task<Module> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un módulo por su ruta
        /// </summary>
        /// <param name="route">Ruta del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Módulo encontrado o null</returns>
        Task<Module> GetByRouteAsync(string route, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene módulos principales (sin padre)
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos principales</returns>
        Task<IReadOnlyList<Module>> GetMainModulesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene submódulos de un módulo padre
        /// </summary>
        /// <param name="parentId">ID del módulo padre</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de submódulos</returns>
        Task<IReadOnlyList<Module>> GetSubmodulesAsync(Guid parentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene módulos con sus permisos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos con sus permisos</returns>
        Task<IReadOnlyList<Module>> GetWithPermissionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un módulo con sus permisos
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Módulo con sus permisos</returns>
        Task<Module> GetWithPermissionsAsync(Guid moduleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene módulos accesibles para un usuario según sus permisos
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos accesibles</returns>
        Task<IReadOnlyList<Module>> GetAccessibleByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
