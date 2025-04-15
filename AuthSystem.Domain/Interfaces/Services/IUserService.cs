using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de usuarios
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Crea un nuevo usuario externo
        /// </summary>
        /// <param name="user">Entidad de usuario</param>
        /// <param name="password">Contraseña en texto plano</param>
        /// <param name="sendActivationEmail">Indica si se debe enviar un correo de activación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario creado</returns>
        Task<User> CreateExternalUserAsync(
            User user, 
            string password, 
            bool sendActivationEmail = true, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Crea un nuevo usuario interno (LDAP)
        /// </summary>
        /// <param name="user">Entidad de usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario creado</returns>
        Task<User> CreateInternalUserAsync(
            User user, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        /// <param name="user">Entidad de usuario con cambios</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario actualizado</returns>
        Task<User> UpdateUserAsync(
            User user, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina un usuario (marcado como eliminado)
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario fue eliminado correctamente</returns>
        Task<bool> DeleteUserAsync(
            Guid userId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<User> GetUserByIdAsync(
            Guid userId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un usuario por su nombre de usuario
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<User> GetUserByUsernameAsync(
            string username, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un usuario por su correo electrónico
        /// </summary>
        /// <param name="email">Correo electrónico</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario encontrado o null</returns>
        Task<User> GetUserByEmailAsync(
            string email, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios</returns>
        Task<IReadOnlyList<User>> GetAllUsersAsync(
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene usuarios por organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios de la organización</returns>
        Task<IReadOnlyList<User>> GetUsersByOrganizationAsync(
            Guid organizationId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene usuarios por sucursal
        /// </summary>
        /// <param name="branchId">ID de la sucursal</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios de la sucursal</returns>
        Task<IReadOnlyList<User>> GetUsersByBranchAsync(
            Guid branchId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene usuarios por rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios con el rol especificado</returns>
        Task<IReadOnlyList<User>> GetUsersByRoleAsync(
            Guid roleId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene usuarios por tipo
        /// </summary>
        /// <param name="userType">Tipo de usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios del tipo especificado</returns>
        Task<IReadOnlyList<User>> GetUsersByTypeAsync(
            UserType userType, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene usuarios por estado
        /// </summary>
        /// <param name="status">Estado del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios en el estado especificado</returns>
        Task<IReadOnlyList<User>> GetUsersByStatusAsync(
            UserStatus status, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Activa una cuenta de usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="activationToken">Token de activación</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la cuenta fue activada correctamente</returns>
        Task<bool> ActivateUserAccountAsync(
            Guid userId, 
            string activationToken, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Bloquea una cuenta de usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="reason">Razón del bloqueo</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la cuenta fue bloqueada correctamente</returns>
        Task<bool> LockUserAccountAsync(
            Guid userId, 
            string reason, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Desbloquea una cuenta de usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la cuenta fue desbloqueada correctamente</returns>
        Task<bool> UnlockUserAccountAsync(
            Guid userId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asigna roles a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleIds">Lista de IDs de roles</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-rol creadas</returns>
        Task<IReadOnlyList<UserRole>> AssignRolesToUserAsync(
            Guid userId, 
            IEnumerable<Guid> roleIds, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina roles de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleIds">Lista de IDs de roles a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de relaciones eliminadas</returns>
        Task<int> RemoveRolesFromUserAsync(
            Guid userId, 
            IEnumerable<Guid> roleIds, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Asigna sucursales a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="branchIds">Lista de IDs de sucursales</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de relaciones usuario-sucursal creadas</returns>
        Task<IReadOnlyList<UserBranch>> AssignBranchesToUserAsync(
            Guid userId, 
            IEnumerable<Guid> branchIds, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina sucursales de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="branchIds">Lista de IDs de sucursales a eliminar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de relaciones eliminadas</returns>
        Task<int> RemoveBranchesFromUserAsync(
            Guid userId, 
            IEnumerable<Guid> branchIds, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca usuarios con criterios avanzados
        /// </summary>
        /// <param name="searchTerm">Término de búsqueda (nombre, correo, etc.)</param>
        /// <param name="userType">Tipo de usuario (opcional)</param>
        /// <param name="status">Estado del usuario (opcional)</param>
        /// <param name="organizationId">ID de la organización (opcional)</param>
        /// <param name="branchId">ID de la sucursal (opcional)</param>
        /// <param name="roleId">ID del rol (opcional)</param>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista paginada de usuarios que cumplen los criterios</returns>
        Task<(IReadOnlyList<User> Items, int TotalCount)> SearchUsersAsync(
            string searchTerm = null,
            UserType? userType = null,
            UserStatus? status = null,
            Guid? organizationId = null,
            Guid? branchId = null,
            Guid? roleId = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
    }
}
