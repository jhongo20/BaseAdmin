using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de usuarios
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        /// <summary>
        /// Obtiene un usuario por su nombre de usuario
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un usuario por su correo electrónico
        /// </summary>
        /// <param name="email">Correo electrónico</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un usuario por su identificador LDAP
        /// </summary>
        /// <param name="ldapDN">Identificador LDAP</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<User> GetByLdapDNAsync(string ldapDN, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene usuarios por organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios de la organización</returns>
        Task<IReadOnlyList<User>> GetByOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene usuarios por sucursal
        /// </summary>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios de la sucursal</returns>
        Task<IReadOnlyList<User>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene usuarios por rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios con el rol especificado</returns>
        Task<IReadOnlyList<User>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un usuario con sus roles y permisos
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario con sus roles y permisos</returns>
        Task<User> GetWithRolesAndPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
