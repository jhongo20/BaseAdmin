using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de autenticación LDAP/Active Directory
    /// </summary>
    public interface ILdapService
    {
        /// <summary>
        /// Autentica un usuario contra LDAP/Active Directory
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="password">Contraseña</param>
        /// <param name="organizationId">ID de la organización (para configuración LDAP)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la autenticación es exitosa</returns>
        Task<bool> AuthenticateAsync(
            string username, 
            string password, 
            Guid organizationId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene información de un usuario LDAP
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="organizationId">ID de la organización (para configuración LDAP)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Información del usuario LDAP</returns>
        Task<LdapUserInfo> GetUserInfoAsync(
            string username, 
            Guid organizationId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca usuarios en LDAP/Active Directory
        /// </summary>
        /// <param name="searchQuery">Consulta de búsqueda</param>
        /// <param name="organizationId">ID de la organización (para configuración LDAP)</param>
        /// <param name="maxResults">Número máximo de resultados</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de usuarios LDAP que coinciden con la búsqueda</returns>
        Task<IReadOnlyList<LdapUserInfo>> SearchUsersAsync(
            string searchQuery, 
            Guid organizationId, 
            int maxResults = 100, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los grupos de un usuario LDAP
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="organizationId">ID de la organización (para configuración LDAP)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de grupos LDAP a los que pertenece el usuario</returns>
        Task<IReadOnlyList<string>> GetUserGroupsAsync(
            string username, 
            Guid organizationId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si un usuario pertenece a un grupo LDAP
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="groupName">Nombre del grupo</param>
        /// <param name="organizationId">ID de la organización (para configuración LDAP)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si el usuario pertenece al grupo</returns>
        Task<bool> IsUserInGroupAsync(
            string username, 
            string groupName, 
            Guid organizationId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Sincroniza un usuario LDAP con el sistema local
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="organizationId">ID de la organización (para configuración LDAP)</param>
        /// <param name="syncGroups">Indica si se deben sincronizar los grupos como roles</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Usuario sincronizado</returns>
        Task<User> SyncUserAsync(
            string username, 
            Guid organizationId, 
            bool syncGroups = true, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica la conexión LDAP
        /// </summary>
        /// <param name="organizationId">ID de la organización (para configuración LDAP)</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la conexión es exitosa</returns>
        Task<bool> TestConnectionAsync(
            Guid organizationId, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Configura la conexión LDAP para una organización
        /// </summary>
        /// <param name="organizationId">ID de la organización</param>
        /// <param name="server">Servidor LDAP</param>
        /// <param name="port">Puerto LDAP</param>
        /// <param name="baseDn">DN base LDAP</param>
        /// <param name="username">Usuario LDAP para conexión</param>
        /// <param name="password">Contraseña LDAP para conexión</param>
        /// <param name="useSSL">Indica si se debe usar SSL</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>True si la configuración fue exitosa</returns>
        Task<bool> ConfigureConnectionAsync(
            Guid organizationId,
            string server,
            int port,
            string baseDn,
            string username,
            string password,
            bool useSSL,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Clase para representar la información de un usuario LDAP
    /// </summary>
    public class LdapUserInfo
    {
        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Nombre completo
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Correo electrónico
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Nombre
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Apellido
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Teléfono
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Departamento
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Título o cargo
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Nombre distintivo (DN)
        /// </summary>
        public string DistinguishedName { get; set; }

        /// <summary>
        /// Grupos a los que pertenece
        /// </summary>
        public IReadOnlyList<string> Groups { get; set; }

        /// <summary>
        /// Atributos adicionales
        /// </summary>
        public Dictionary<string, string> AdditionalAttributes { get; set; }
    }
}
