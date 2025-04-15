using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de organizaciones
    /// </summary>
    public interface IOrganizationService
    {
        /// <summary>
        /// Crea una nueva organización
        /// </summary>
        /// <param name="organization">Entidad de organización</param>
        /// <param name="createDefaultRoles">Indica si se deben crear roles predeterminados</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización creada</returns>
        Task<Organization> CreateOrganizationAsync(
            Organization organization, 
            bool createDefaultRoles = true, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza una organización existente
        /// </summary>
        /// <param name="organization">Entidad de organización con cambios</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización actualizada</returns>
        Task<Organization> UpdateOrganizationAsync(
            Organization organization, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina una organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la organización fue eliminada correctamente</returns>
        Task<bool> DeleteOrganizationAsync(
            Guid organizationId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una organización por su ID
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="includeBranches">Indica si se deben incluir las sucursales</param>
        /// <param name="includeRoles">Indica si se deben incluir los roles</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización encontrada o null</returns>
        Task<Organization> GetOrganizationByIdAsync(
            Guid organizationId, 
            bool includeBranches = false, 
            bool includeRoles = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una organización por su nombre
        /// </summary>
        /// <param name="name">Nombre de la organización</param>
        /// <param name="includeBranches">Indica si se deben incluir las sucursales</param>
        /// <param name="includeRoles">Indica si se deben incluir los roles</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización encontrada o null</returns>
        Task<Organization> GetOrganizationByNameAsync(
            string name, 
            bool includeBranches = false, 
            bool includeRoles = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una organización por su identificación fiscal
        /// </summary>
        /// <param name="taxId">Identificación fiscal</param>
        /// <param name="includeBranches">Indica si se deben incluir las sucursales</param>
        /// <param name="includeRoles">Indica si se deben incluir los roles</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Organización encontrada o null</returns>
        Task<Organization> GetOrganizationByTaxIdAsync(
            string taxId, 
            bool includeBranches = false, 
            bool includeRoles = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todas las organizaciones
        /// </summary>
        /// <param name="includeBranches">Indica si se deben incluir las sucursales</param>
        /// <param name="includeRoles">Indica si se deben incluir los roles</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de organizaciones</returns>
        Task<IReadOnlyList<Organization>> GetAllOrganizationsAsync(
            bool includeBranches = false, 
            bool includeRoles = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las organizaciones a las que pertenece un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="includeBranches">Indica si se deben incluir las sucursales</param>
        /// <param name="includeRoles">Indica si se deben incluir los roles</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de organizaciones del usuario</returns>
        Task<IReadOnlyList<Organization>> GetOrganizationsByUserAsync(
            Guid userId, 
            bool includeBranches = false, 
            bool includeRoles = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Crea una nueva sucursal
        /// </summary>
        /// <param name="branch">Entidad de sucursal</param>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sucursal creada</returns>
        Task<Branch> CreateBranchAsync(
            Branch branch, 
            Guid organizationId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza una sucursal existente
        /// </summary>
        /// <param name="branch">Entidad de sucursal con cambios</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sucursal actualizada</returns>
        Task<Branch> UpdateBranchAsync(
            Branch branch, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina una sucursal
        /// </summary>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la sucursal fue eliminada correctamente</returns>
        Task<bool> DeleteBranchAsync(
            Guid branchId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una sucursal por su ID
        /// </summary>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="includeUsers">Indica si se deben incluir los usuarios</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sucursal encontrada o null</returns>
        Task<Branch> GetBranchByIdAsync(
            Guid branchId, 
            bool includeUsers = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una sucursal por su código
        /// </summary>
        /// <param name="code">Código de la sucursal</param>
        /// <param name="includeUsers">Indica si se deben incluir los usuarios</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Sucursal encontrada o null</returns>
        Task<Branch> GetBranchByCodeAsync(
            string code, 
            bool includeUsers = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene sucursales por organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="includeUsers">Indica si se deben incluir los usuarios</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sucursales de la organización</returns>
        Task<IReadOnlyList<Branch>> GetBranchesByOrganizationAsync(
            Guid organizationId, 
            bool includeUsers = false, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene sucursales por usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de sucursales del usuario</returns>
        Task<IReadOnlyList<Branch>> GetBranchesByUserAsync(
            Guid userId, 
            CancellationToken cancellationToken = default);

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
        Task<(IReadOnlyList<Organization> Items, int TotalCount)> SearchOrganizationsAsync(
            string searchTerm = null,
            bool? isActive = null,
            bool includeBranches = false,
            bool includeRoles = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca sucursales con criterios avanzados
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (nombre, código)</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="isActive">Indica si está activa (opcional)</param>
        /// <param name="includeUsers">Indica si se deben incluir los usuarios</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de sucursales que cumplen los criterios</returns>
        Task<(IReadOnlyList<Branch> Items, int TotalCount)> SearchBranchesAsync(
            string searchTerm = null,
            Guid? organizationId = null,
            bool? isActive = null,
            bool includeUsers = false,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

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
        Task<bool> ConfigureLdapConnectionAsync(
            Guid organizationId,
            string ldapServer,
            int ldapPort,
            string ldapBaseDn,
            string ldapUsername,
            string ldapPassword,
            bool ldapUseSSL,
            CancellationToken cancellationToken = default);
    }
}
