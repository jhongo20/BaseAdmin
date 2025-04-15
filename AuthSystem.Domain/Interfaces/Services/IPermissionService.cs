using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de permisos
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// Crea un nuevo permiso
        /// </summary>
        /// <param name="permission">Entidad de permiso</param>
        /// <param name="moduleId">ID del módulo al que pertenece</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Permiso creado</returns>
        Task<Permission> CreatePermissionAsync(
            Permission permission, 
            Guid moduleId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un permiso existente
        /// </summary>
        /// <param name="permission">Entidad de permiso con cambios</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Permiso actualizado</returns>
        Task<Permission> UpdatePermissionAsync(
            Permission permission, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina un permiso
        /// </summary>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el permiso fue eliminado correctamente</returns>
        Task<bool> DeletePermissionAsync(
            Guid permissionId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un permiso por su ID
        /// </summary>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Permiso encontrado o null</returns>
        Task<Permission> GetPermissionByIdAsync(
            Guid permissionId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un permiso por su nombre
        /// </summary>
        /// <param name="name">Nombre del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Permiso encontrado o null</returns>
        Task<Permission> GetPermissionByNameAsync(
            string name, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los permisos
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos</returns>
        Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene permisos por módulo
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos del módulo</returns>
        Task<IReadOnlyList<Permission>> GetPermissionsByModuleAsync(
            Guid moduleId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene permisos por rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos del rol</returns>
        Task<IReadOnlyList<Permission>> GetPermissionsByRoleAsync(
            Guid roleId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene permisos por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos del usuario</returns>
        Task<IReadOnlyList<Permission>> GetPermissionsByUserAsync(
            Guid userId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene permisos por tipo
        /// </summary>
        /// <param name="permissionType">Tipo de permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos del tipo especificado</returns>
        Task<IReadOnlyList<Permission>> GetPermissionsByTypeAsync(
            PermissionType permissionType, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="permissionId">ID del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario tiene el permiso</returns>
        Task<bool> UserHasPermissionAsync(
            Guid userId, 
            Guid permissionId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un usuario tiene un permiso específico por nombre
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="permissionName">Nombre del permiso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario tiene el permiso</returns>
        Task<bool> UserHasPermissionByNameAsync(
            Guid userId, 
            string permissionName, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca permisos con criterios avanzados
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (nombre, descripción)</param>
        /// <param name="moduleId">ID del módulo (opcional)</param>
        /// <param name="permissionType">Tipo de permiso (opcional)</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de permisos que cumplen los criterios</returns>
        Task<(IReadOnlyList<Permission> Items, int TotalCount)> SearchPermissionsAsync(
            string searchTerm = null,
            Guid? moduleId = null,
            PermissionType? permissionType = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Crea permisos estándar para un módulo
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="permissionTypes">Tipos de permisos a crear</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de permisos creados</returns>
        Task<IReadOnlyList<Permission>> CreateStandardPermissionsForModuleAsync(
            Guid moduleId,
            IEnumerable<PermissionType> permissionTypes,
            CancellationToken cancellationToken = default);
    }
}
