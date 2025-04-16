using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Repositories;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AuthSystem.Infrastructure.Services
{
    public class LdapService : ILdapService
    {
        private readonly IConfiguration _configuration;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly ILogger<LdapService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly CacheSettings _cacheSettings;

        public LdapService(
            IConfiguration configuration,
            IOrganizationRepository organizationRepository,
            ILogger<LdapService> logger,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IUnitOfWork unitOfWork,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings)
        {
            _configuration = configuration;
            _organizationRepository = organizationRepository;
            _logger = logger;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _cacheSettings = cacheSettings.Value;
        }

        public async Task<bool> AuthenticateAsync(
            string username,
            string password,
            Guid organizationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var ldapConfig = await GetLdapConfigAsync(organizationId, cancellationToken);
                if (ldapConfig == null)
                {
                    _logger.LogWarning("LDAP configuration not found for organization {OrganizationId}", organizationId);
                    return false;
                }

                using var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapConfig.Server, ldapConfig.Port));
                connection.AuthType = AuthType.Basic;
                
                // Construir el DN completo para el usuario
                var userDn = string.Format(ldapConfig.UserDnFormat, username);
                
                // Intentar autenticar
                connection.Bind(new NetworkCredential(userDn, password));
                
                return true;
            }
            catch (LdapException ex)
            {
                _logger.LogWarning(ex, "LDAP authentication failed for user {Username}", username);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during LDAP authentication for user {Username}", username);
                return false;
            }
        }

        public async Task<LdapUserInfo> GetUserInfoAsync(
            string username,
            Guid organizationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Intentar obtener de la caché primero
                string cacheKey = $"ldap:userinfo:{organizationId}:{username}";
                var cachedUserInfo = await _cacheService.GetAsync<LdapUserInfo>(cacheKey, cancellationToken);
                if (cachedUserInfo != null)
                {
                    _logger.LogDebug("LDAP user info retrieved from cache for {Username}", username);
                    return cachedUserInfo;
                }

                var ldapConfig = await GetLdapConfigAsync(organizationId, cancellationToken);
                if (ldapConfig == null)
                {
                    _logger.LogWarning("LDAP configuration not found for organization {OrganizationId}", organizationId);
                    return null;
                }

                using var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapConfig.Server, ldapConfig.Port));
                connection.AuthType = AuthType.Basic;
                connection.Bind(new NetworkCredential(ldapConfig.BindDn, ldapConfig.BindPassword));

                // Crear la solicitud de búsqueda
                var searchFilter = string.Format(ldapConfig.SearchFilter, username);
                var searchRequest = new SearchRequest(
                    ldapConfig.SearchBase,
                    searchFilter,
                    SearchScope.Subtree,
                    "cn", "mail", "givenName", "sn", "distinguishedName", "memberOf", "objectGUID");

                // Ejecutar la búsqueda
                var response = (SearchResponse)connection.SendRequest(searchRequest);
                
                if (response.Entries.Count == 0)
                {
                    _logger.LogWarning("User {Username} not found in LDAP", username);
                    return null;
                }

                var entry = response.Entries[0];
                
                // Extraer la información del usuario
                var userInfo = new LdapUserInfo
                {
                    Username = username,
                    Email = GetAttributeValue(entry, "mail"),
                    FirstName = GetAttributeValue(entry, "givenName"),
                    LastName = GetAttributeValue(entry, "sn"),
                    FullName = GetAttributeValue(entry, "cn"),
                    DistinguishedName = GetAttributeValue(entry, "distinguishedName"),
                    Groups = GetAttributeValues(entry, "memberOf")
                };

                // Guardar en caché
                TimeSpan cacheExpiration = TimeSpan.FromMinutes(_cacheSettings.LdapCacheAbsoluteExpirationMinutes);
                await _cacheService.SetAsync(cacheKey, userInfo, cacheExpiration, null, cancellationToken);
                _logger.LogDebug("LDAP user info stored in cache for {Username}", username);

                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting LDAP user info for {Username}", username);
                return null;
            }
        }

        public async Task<IReadOnlyList<LdapUserInfo>> SearchUsersAsync(
            string searchQuery,
            Guid organizationId,
            int maxResults = 100,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var ldapConfig = await GetLdapConfigAsync(organizationId, cancellationToken);
                if (ldapConfig == null)
                {
                    _logger.LogWarning("LDAP configuration not found for organization {OrganizationId}", organizationId);
                    return new List<LdapUserInfo>();
                }

                using var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapConfig.Server, ldapConfig.Port));
                connection.AuthType = AuthType.Basic;
                connection.Bind(new NetworkCredential(ldapConfig.BindDn, ldapConfig.BindPassword));

                // Crear la solicitud de búsqueda
                var searchFilter = $"(&(objectClass=person)(|(cn=*{searchQuery}*)(sAMAccountName=*{searchQuery}*)(mail=*{searchQuery}*)))";
                var searchRequest = new SearchRequest(
                    ldapConfig.SearchBase,
                    searchFilter,
                    SearchScope.Subtree,
                    "cn", "mail", "givenName", "sn", "distinguishedName", "sAMAccountName");

                // Configurar límite de resultados
                var searchOptions = new SearchOptionsControl(System.DirectoryServices.Protocols.SearchOption.DomainScope);
                searchRequest.Controls.Add(searchOptions);
                
                // Ejecutar la búsqueda
                var response = (SearchResponse)connection.SendRequest(searchRequest);
                
                var users = new List<LdapUserInfo>();
                foreach (SearchResultEntry entry in response.Entries)
                {
                    if (users.Count >= maxResults)
                        break;

                    var userInfo = new LdapUserInfo
                    {
                        Username = GetAttributeValue(entry, "sAMAccountName"),
                        Email = GetAttributeValue(entry, "mail"),
                        FirstName = GetAttributeValue(entry, "givenName"),
                        LastName = GetAttributeValue(entry, "sn"),
                        FullName = GetAttributeValue(entry, "cn"),
                        DistinguishedName = GetAttributeValue(entry, "distinguishedName")
                    };
                    
                    users.Add(userInfo);
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching LDAP users with query {SearchQuery}", searchQuery);
                return new List<LdapUserInfo>();
            }
        }

        public async Task<bool> SynchronizeUserAsync(
            string username,
            Guid organizationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userInfo = await GetUserInfoAsync(username, organizationId, cancellationToken);
                if (userInfo == null)
                {
                    _logger.LogWarning("Cannot synchronize user {Username}, not found in LDAP", username);
                    return false;
                }

                // Aquí iría la lógica para sincronizar el usuario con la base de datos
                // Por ejemplo, actualizar o crear el usuario en la base de datos local
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing LDAP user {Username}", username);
                return false;
            }
        }

        public async Task<IReadOnlyList<string>> GetUserGroupsAsync(
            string username, 
            Guid organizationId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Obteniendo grupos LDAP para el usuario {Username}", username);
                
                var ldapConfig = await GetLdapConfigAsync(organizationId, cancellationToken);
                if (ldapConfig == null)
                {
                    _logger.LogWarning("No se encontró configuración LDAP para la organización {OrganizationId}", organizationId);
                    return new List<string>();
                }

                var connection = await ConfigureConnectionAsync(ldapConfig, cancellationToken);
                var userDn = await GetUserDnAsync(username, ldapConfig, connection, cancellationToken);
                
                if (string.IsNullOrEmpty(userDn))
                {
                    _logger.LogWarning("No se encontró el usuario {Username} en LDAP", username);
                    return new List<string>();
                }

                // Buscar grupos a los que pertenece el usuario
                var searchRequest = new SearchRequest(
                    ldapConfig.BaseDN,
                    $"(&(objectClass=group)(member:1.2.840.113556.1.4.1941:={userDn}))",
                    System.DirectoryServices.Protocols.SearchScope.Subtree,
                    "cn");

                var response = (SearchResponse)connection.SendRequest(searchRequest);
                
                var groups = new List<string>();
                foreach (SearchResultEntry entry in response.Entries)
                {
                    var groupName = GetAttributeValue(entry, "cn");
                    if (!string.IsNullOrEmpty(groupName))
                    {
                        groups.Add(groupName);
                    }
                }

                _logger.LogInformation("Se encontraron {Count} grupos para el usuario {Username}", groups.Count, username);
                return groups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener grupos LDAP para el usuario {Username}", username);
                return new List<string>();
            }
        }

        public async Task<bool> IsUserInGroupAsync(
            string username,
            string groupName,
            Guid organizationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var groups = await GetUserGroupsAsync(username, organizationId, cancellationToken);
                return groups.Contains(groupName, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si el usuario {Username} pertenece al grupo {GroupName} para la organización {OrganizationId}", 
                    username, groupName, organizationId);
                return false;
            }
        }

        public async Task<User> SyncUserAsync(
            string username,
            Guid organizationId,
            bool syncGroups = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Obtener información del usuario LDAP
                var ldapUserInfo = await GetUserInfoAsync(username, organizationId, cancellationToken);
                if (ldapUserInfo == null)
                {
                    _logger.LogWarning("Usuario LDAP {Username} no encontrado para la organización {OrganizationId}", username, organizationId);
                    return null;
                }

                // Buscar si el usuario ya existe en el sistema
                var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
                
                if (user == null)
                {
                    // Crear nuevo usuario
                    user = new User
                    {
                        Username = username,
                        Email = ldapUserInfo.Email,
                        FullName = $"{ldapUserInfo.FirstName} {ldapUserInfo.LastName}",
                        UserType = Domain.Common.Enums.UserType.Internal,
                        Status = Domain.Common.Enums.UserStatus.Active,
                        IsActive = true,
                        LdapDN = ldapUserInfo.DistinguishedName,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    await _userRepository.AddAsync(user, cancellationToken);
                }
                else
                {
                    // Actualizar usuario existente
                    user.Email = ldapUserInfo.Email;
                    user.FullName = $"{ldapUserInfo.FirstName} {ldapUserInfo.LastName}";
                    user.UserType = Domain.Common.Enums.UserType.Internal;
                    user.LdapDN = ldapUserInfo.DistinguishedName;
                    user.LastModifiedAt = DateTime.UtcNow;
                    
                    await _userRepository.UpdateAsync(user, cancellationToken);
                }

                // Sincronizar grupos como roles si se solicita
                if (syncGroups)
                {
                    var groups = await GetUserGroupsAsync(username, organizationId, cancellationToken);
                    
                    foreach (var groupName in groups)
                    {
                        // Buscar o crear rol correspondiente al grupo
                        var role = await _roleRepository.GetByNameAsync(groupName, organizationId, cancellationToken);
                        
                        if (role == null)
                        {
                            role = new Role
                            {
                                Name = groupName,
                                Description = $"Rol sincronizado desde LDAP para el grupo {groupName}",
                                OrganizationId = organizationId,
                                CreatedAt = DateTime.UtcNow
                            };
                            
                            await _roleRepository.AddAsync(role, cancellationToken);
                        }

                        // Asignar rol al usuario si no lo tiene ya
                        var userRole = await _userRoleRepository.GetByUserAndRoleAsync(role.Id, user.Id, cancellationToken);
                        
                        if (userRole == null)
                        {
                            userRole = new UserRole
                            {
                                UserId = user.Id,
                                RoleId = role.Id,
                                CreatedAt = DateTime.UtcNow
                            };
                            
                            await _userRoleRepository.AddAsync(userRole, cancellationToken);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sincronizar usuario LDAP {Username} para la organización {OrganizationId}", 
                    username, organizationId);
                return null;
            }
        }

        public async Task<bool> TestConnectionAsync(
            Guid organizationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new LdapConnection(new LdapDirectoryIdentifier(_configuration["LdapSettings:Server"], int.Parse(_configuration["LdapSettings:Port"]))))
                {
                    connection.SessionOptions.ProtocolVersion = 3;
                    
                    // Conectar con credenciales de administrador
                    connection.Bind(new NetworkCredential(_configuration["LdapSettings:BindDN"], _configuration["LdapSettings:BindPassword"]));
                    
                    // Si llegamos aquí, la conexión fue exitosa
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al probar conexión LDAP para la organización {OrganizationId}", organizationId);
                return false;
            }
        }

        public async Task<bool> ConfigureConnectionAsync(
            Guid organizationId,
            string server,
            int port,
            string bindDn,
            string bindPassword,
            string searchBase,
            bool useSSL,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // En una implementación real, aquí se guardaría la configuración LDAP para la organización
                // Por ahora, solo probamos la conexión con los parámetros proporcionados
                
                using (var connection = new LdapConnection(new LdapDirectoryIdentifier(server, port)))
                {
                    connection.SessionOptions.ProtocolVersion = 3;
                    
                    if (useSSL)
                    {
                        connection.SessionOptions.SecureSocketLayer = true;
                    }
                    
                    // Conectar con las credenciales proporcionadas
                    connection.Bind(new NetworkCredential(bindDn, bindPassword));
                    
                    // Realizar una búsqueda simple para verificar que todo funciona
                    var searchRequest = new SearchRequest(
                        searchBase,
                        "(objectClass=*)",
                        SearchScope.Base,
                        null);
                    
                    var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
                    
                    // Si llegamos aquí, la configuración es válida
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar conexión LDAP para la organización {OrganizationId}", organizationId);
                return false;
            }
        }

        private async Task<LdapConfig> GetLdapConfigAsync(Guid organizationId, CancellationToken cancellationToken)
        {
            // Intentar obtener de la caché primero
            string cacheKey = $"ldap:config:{organizationId}";
            var cachedConfig = await _cacheService.GetAsync<LdapConfig>(cacheKey, cancellationToken);
            if (cachedConfig != null)
            {
                _logger.LogDebug("LDAP configuration retrieved from cache for organization {OrganizationId}", organizationId);
                return cachedConfig;
            }

            // En un escenario real, obtendríamos la configuración LDAP específica de la organización
            // desde la base de datos o un servicio de configuración
            
            // Para simplificar, usamos la configuración global
            var config = new LdapConfig
            {
                Server = _configuration["LdapSettings:Server"],
                Port = int.Parse(_configuration["LdapSettings:Port"]),
                BindDn = _configuration["LdapSettings:BindDN"],
                BindPassword = _configuration["LdapSettings:BindPassword"],
                SearchBase = _configuration["LdapSettings:SearchBase"],
                SearchFilter = _configuration["LdapSettings:SearchFilter"],
                UserDnFormat = "uid={0}," + _configuration["LdapSettings:SearchBase"]
            };

            // Guardar en caché
            TimeSpan cacheExpiration = TimeSpan.FromMinutes(_cacheSettings.ConfigurationCacheAbsoluteExpirationMinutes);
            await _cacheService.SetAsync(cacheKey, config, cacheExpiration, null, cancellationToken);
            _logger.LogDebug("LDAP configuration stored in cache for organization {OrganizationId}", organizationId);

            return config;
        }

        private string GetAttributeValue(SearchResultEntry entry, string attributeName)
        {
            if (entry.Attributes.Contains(attributeName) && entry.Attributes[attributeName].Count > 0)
            {
                return entry.Attributes[attributeName][0].ToString();
            }
            return string.Empty;
        }

        private List<string> GetAttributeValues(SearchResultEntry entry, string attributeName)
        {
            var values = new List<string>();
            if (entry.Attributes.Contains(attributeName))
            {
                for (int i = 0; i < entry.Attributes[attributeName].Count; i++)
                {
                    values.Add(entry.Attributes[attributeName][i].ToString());
                }
            }
            return values;
        }

        private async Task<string> GetUserDnAsync(
            string username,
            Guid organizationId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new LdapConnection(new LdapDirectoryIdentifier(_configuration["LdapSettings:Server"], int.Parse(_configuration["LdapSettings:Port"]))))
                {
                    connection.SessionOptions.ProtocolVersion = 3;
                    
                    // Conectar con credenciales de administrador
                    connection.Bind(new NetworkCredential(_configuration["LdapSettings:BindDN"], _configuration["LdapSettings:BindPassword"]));
                    
                    // Buscar el DN del usuario
                    var searchFilter = string.Format(_configuration["LdapSettings:SearchFilter"], username);
                    var searchRequest = new SearchRequest(
                        _configuration["LdapSettings:SearchBase"],
                        searchFilter,
                        SearchScope.Subtree,
                        new string[] { "distinguishedName" });
                    
                    var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
                    
                    if (searchResponse.Entries.Count > 0)
                    {
                        var entry = searchResponse.Entries[0];
                        if (entry.Attributes.Contains("distinguishedName"))
                        {
                            return entry.Attributes["distinguishedName"][0].ToString();
                        }
                    }
                    
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener DN del usuario {Username} para la organización {OrganizationId}", 
                    username, organizationId);
                return null;
            }
        }

        private async Task<string> GetUserDnAsync(
            string username,
            LdapConfig ldapConfig,
            LdapConnection connection,
            CancellationToken cancellationToken)
        {
            try
            {
                // Buscar el DN del usuario
                var searchFilter = string.Format(ldapConfig.SearchFilter, username);
                var searchRequest = new SearchRequest(
                    ldapConfig.SearchBase,
                    searchFilter,
                    SearchScope.Subtree,
                    new string[] { "distinguishedName" });
                
                var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
                
                if (searchResponse.Entries.Count > 0)
                {
                    var entry = searchResponse.Entries[0];
                    if (entry.Attributes.Contains("distinguishedName"))
                    {
                        return entry.Attributes["distinguishedName"][0].ToString();
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener DN del usuario {Username}", username);
                return null;
            }
        }

        private async Task<LdapConnection> ConfigureConnectionAsync(
            LdapConfig ldapConfig,
            CancellationToken cancellationToken)
        {
            try
            {
                var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapConfig.Server, ldapConfig.Port));
                connection.SessionOptions.ProtocolVersion = 3;
                
                // Conectar con credenciales de administrador
                connection.Bind(new NetworkCredential(ldapConfig.BindDn, ldapConfig.BindPassword));
                
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al configurar conexión LDAP");
                return null;
            }
        }
    }

    public class LdapConfig
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string BindDn { get; set; }
        public string BindPassword { get; set; }
        public string SearchBase { get; set; }
        public string SearchFilter { get; set; }
        public string UserDnFormat { get; set; }
        public string BaseDN { get; set; }
    }
}
