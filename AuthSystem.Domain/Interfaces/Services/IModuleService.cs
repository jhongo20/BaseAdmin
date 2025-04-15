using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de módulos
    /// </summary>
    public interface IModuleService
    {
        /// <summary>
        /// Crea un nuevo módulo
        /// </summary>
        /// <param name="module">Entidad de módulo</param>
        /// <param name="parentModuleId">ID del módulo padre (opcional)</param>
        /// <param name="createStandardPermissions">Indica si se deben crear permisos estándar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Módulo creado</returns>
        Task<Module> CreateModuleAsync(
            Module module, 
            Guid? parentModuleId = null, 
            bool createStandardPermissions = true, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un módulo existente
        /// </summary>
        /// <param name="module">Entidad de módulo con cambios</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Módulo actualizado</returns>
        Task<Module> UpdateModuleAsync(
            Module module, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina un módulo
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="deleteChildModules">Indica si se deben eliminar los módulos hijos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el módulo fue eliminado correctamente</returns>
        Task<bool> DeleteModuleAsync(
            Guid moduleId, 
            bool deleteChildModules = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un módulo por su ID
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="includeChildModules">Indica si se deben incluir los módulos hijos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Módulo encontrado o null</returns>
        Task<Module> GetModuleByIdAsync(
            Guid moduleId, 
            bool includePermissions = false, 
            bool includeChildModules = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un módulo por su nombre
        /// </summary>
        /// <param name="name">Nombre del módulo</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="includeChildModules">Indica si se deben incluir los módulos hijos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Módulo encontrado o null</returns>
        Task<Module> GetModuleByNameAsync(
            string name, 
            bool includePermissions = false, 
            bool includeChildModules = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un módulo por su ruta
        /// </summary>
        /// <param name="route">Ruta del módulo</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="includeChildModules">Indica si se deben incluir los módulos hijos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Módulo encontrado o null</returns>
        Task<Module> GetModuleByRouteAsync(
            string route, 
            bool includePermissions = false, 
            bool includeChildModules = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los módulos
        /// </summary>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="includeChildModules">Indica si se deben incluir los módulos hijos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos</returns>
        Task<IReadOnlyList<Module>> GetAllModulesAsync(
            bool includePermissions = false, 
            bool includeChildModules = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los módulos principales (sin módulo padre)
        /// </summary>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="includeChildModules">Indica si se deben incluir los módulos hijos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos principales</returns>
        Task<IReadOnlyList<Module>> GetMainModulesAsync(
            bool includePermissions = false, 
            bool includeChildModules = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los módulos hijos de un módulo
        /// </summary>
        /// <param name="parentModuleId">ID del módulo padre</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="includeChildModules">Indica si se deben incluir los módulos hijos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos hijos</returns>
        Task<IReadOnlyList<Module>> GetChildModulesAsync(
            Guid parentModuleId, 
            bool includePermissions = false, 
            bool includeChildModules = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los módulos accesibles para un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="includeChildModules">Indica si se deben incluir los módulos hijos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de módulos accesibles</returns>
        Task<IReadOnlyList<Module>> GetAccessibleModulesForUserAsync(
            Guid userId, 
            bool includePermissions = false, 
            bool includeChildModules = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un usuario tiene acceso a un módulo
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="requiredPermissionType">Tipo de permiso requerido (opcional)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario tiene acceso al módulo</returns>
        Task<bool> UserHasModuleAccessAsync(
            Guid userId, 
            Guid moduleId, 
            PermissionType? requiredPermissionType = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca módulos con criterios avanzados
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (nombre, descripción, ruta)</param>
        /// <param name="parentModuleId">ID del módulo padre (opcional)</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="includeChildModules">Indica si se deben incluir los módulos hijos</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de módulos que cumplen los criterios</returns>
        Task<(IReadOnlyList<Module> Items, int TotalCount)> SearchModulesAsync(
            string searchTerm = null,
            Guid? parentModuleId = null,
            bool includePermissions = false,
            bool includeChildModules = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el árbol completo de módulos
        /// </summary>
        /// <param name="includePermissions">Indica si se deben incluir los permisos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Árbol de módulos</returns>
        Task<IReadOnlyList<Module>> GetModuleTreeAsync(
            bool includePermissions = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Reordena los módulos hijos de un módulo padre
        /// </summary>
        /// <param name="parentModuleId">ID del módulo padre (null para módulos principales)</param>
        /// <param name="moduleIds">Lista ordenada de IDs de módulos</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la reordenación fue exitosa</returns>
        Task<bool> ReorderModulesAsync(
            Guid? parentModuleId, 
            IList<Guid> moduleIds, 
            CancellationToken cancellationToken = default);
    }
}
