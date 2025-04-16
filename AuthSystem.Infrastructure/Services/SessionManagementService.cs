using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de gestión de sesiones
    /// </summary>
    public class SessionManagementService : ISessionManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SessionManagementService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITokenRevocationService _tokenRevocationService;

        private readonly int _maxConcurrentSessions;
        private readonly int _sessionTimeoutMinutes;
        private readonly bool _enableSessionPersistence;
        private readonly bool _forceLogoutOnPasswordChange;
        private readonly bool _forceLogoutOnRoleChange;
        private readonly bool _trackUserActivity;
        private readonly int _activityTrackingIntervalSeconds;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo</param>
        /// <param name="logger">Logger</param>
        /// <param name="configuration">Configuración</param>
        /// <param name="tokenRevocationService">Servicio de revocación de tokens</param>
        public SessionManagementService(
            IUnitOfWork unitOfWork,
            ILogger<SessionManagementService> logger,
            IConfiguration configuration,
            ITokenRevocationService tokenRevocationService)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tokenRevocationService = tokenRevocationService ?? throw new ArgumentNullException(nameof(tokenRevocationService));

            // Cargar configuración
            _maxConcurrentSessions = _configuration.GetValue<int>("SessionManagement:MaxConcurrentSessions", 5);
            _sessionTimeoutMinutes = _configuration.GetValue<int>("SessionManagement:SessionTimeoutMinutes", 30);
            _enableSessionPersistence = _configuration.GetValue<bool>("SessionManagement:EnableSessionPersistence", true);
            _forceLogoutOnPasswordChange = _configuration.GetValue<bool>("SessionManagement:ForceLogoutOnPasswordChange", true);
            _forceLogoutOnRoleChange = _configuration.GetValue<bool>("SessionManagement:ForceLogoutOnRoleChange", true);
            _trackUserActivity = _configuration.GetValue<bool>("SessionManagement:TrackUserActivity", true);
            _activityTrackingIntervalSeconds = _configuration.GetValue<int>("SessionManagement:ActivityTrackingIntervalSeconds", 60);
        }

        /// <summary>
        /// Crea una nueva sesión para un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="token">Token JWT</param>
        /// <param name="tokenId">ID del token JWT (JTI)</param>
        /// <param name="refreshToken">Token de actualización</param>
        /// <param name="expiresAt">Fecha y hora de expiración del token</param>
        /// <param name="ipAddress">Dirección IP desde la que se inicia la sesión</param>
        /// <param name="userAgent">User-Agent desde el que se inicia la sesión</param>
        /// <returns>Sesión creada</returns>
        public async Task<UserSession> CreateSessionAsync(
            Guid userId,
            string token,
            string tokenId,
            string refreshToken,
            DateTime expiresAt,
            string ipAddress,
            string userAgent)
        {
            try
            {
                // Verificar si el usuario ha excedido el número máximo de sesiones concurrentes
                if (await HasExceededMaxConcurrentSessionsAsync(userId))
                {
                    _logger.LogWarning("El usuario {UserId} ha excedido el número máximo de sesiones concurrentes", userId);
                    
                    // Cerrar la sesión más antigua
                    var activeSessions = await _unitOfWork.UserSessions.GetActiveByUserIdAsync(userId);
                    if (activeSessions.Any())
                    {
                        var oldestSession = activeSessions.OrderBy(s => s.CreatedAt).First();
                        await CloseSessionAsync(oldestSession.Id, "Excedido el número máximo de sesiones concurrentes");
                    }
                }

                // Crear la nueva sesión
                var session = new UserSession
                {
                    UserId = userId,
                    Token = token,
                    TokenId = tokenId,
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = expiresAt,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    LastActivity = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _unitOfWork.UserSessions.AddAsync(session);
                await _unitOfWork.SaveChangesAsync();

                // Actualizar la fecha de último inicio de sesión del usuario
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _unitOfWork.Users.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync();
                }

                _logger.LogInformation("Sesión creada para el usuario {UserId}", userId);
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear sesión para el usuario {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene una sesión por su ID
        /// </summary>
        /// <param name="sessionId">ID de la sesión</param>
        /// <returns>Sesión o null si no existe</returns>
        public async Task<UserSession> GetSessionByIdAsync(Guid sessionId)
        {
            try
            {
                return await _unitOfWork.UserSessions.GetByIdAsync(sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesión {SessionId}", sessionId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene una sesión por su token
        /// </summary>
        /// <param name="token">Token JWT</param>
        /// <returns>Sesión o null si no existe</returns>
        public async Task<UserSession> GetSessionByTokenAsync(string token)
        {
            try
            {
                return await _unitOfWork.UserSessions.GetByTokenAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesión por token");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una sesión por su token ID (JTI)
        /// </summary>
        /// <param name="tokenId">ID del token JWT (JTI)</param>
        /// <returns>Sesión o null si no existe</returns>
        public async Task<UserSession> GetSessionByTokenIdAsync(string tokenId)
        {
            try
            {
                return await _unitOfWork.UserSessions.GetByTokenIdAsync(tokenId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesión por token ID {TokenId}", tokenId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene todas las sesiones activas de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de sesiones activas</returns>
        public async Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId)
        {
            try
            {
                return await _unitOfWork.UserSessions.GetActiveByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener sesiones activas del usuario {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Obtiene todas las sesiones activas
        /// </summary>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <returns>Lista paginada de sesiones activas</returns>
        public async Task<(IReadOnlyList<UserSession> Sessions, int TotalCount)> GetAllActiveSessionsAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                // Validar parámetros de paginación
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > 100) pageSize = 20;

                var totalCount = await _unitOfWork.UserSessions.CountAsync(s => s.IsActive && s.EndedAt == null);
                
                // Obtener sesiones paginadas
                var skip = (page - 1) * pageSize;
                var sessions = await _unitOfWork.UserSessions.GetAllAsync(
                    s => s.IsActive && s.EndedAt == null);
                
                // Aplicar paginación manualmente
                var paginatedSessions = sessions
                    .OrderByDescending(s => s.LastActivity)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToList();

                return (paginatedSessions, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las sesiones activas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el número total de sesiones activas
        /// </summary>
        /// <returns>Número total de sesiones activas</returns>
        public async Task<int> GetTotalActiveSessionsAsync()
        {
            try
            {
                return await _unitOfWork.UserSessions.CountAsync(s => s.IsActive && s.EndedAt == null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el número total de sesiones activas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el número de sesiones activas por rol
        /// </summary>
        /// <returns>Diccionario con el número de sesiones por rol</returns>
        public async Task<Dictionary<string, int>> GetActiveSessionsByRoleAsync()
        {
            try
            {
                var result = new Dictionary<string, int>();
                
                // Obtener todas las sesiones activas
                var activeSessions = await _unitOfWork.UserSessions.GetAllAsync();
                var activeSessionsList = activeSessions.Where(s => s.IsActive && s.EndedAt == null).ToList();
                var userIds = activeSessionsList.Select(s => s.UserId).Distinct().ToList();
                
                // Obtener roles de los usuarios
                foreach (var userId in userIds)
                {
                    // Obtener roles del usuario usando UserRoles
                    var userRoles = await _unitOfWork.UserRoles.GetAllAsync();
                    var userRolesList = userRoles.Where(ur => ur.UserId == userId).ToList();
                    var roleIds = userRolesList.Select(ur => ur.RoleId).ToList();
                    
                    // Obtener nombres de roles
                    foreach (var roleId in roleIds)
                    {
                        var role = await _unitOfWork.Roles.GetByIdAsync(roleId);
                        if (role != null)
                        {
                            if (result.ContainsKey(role.Name))
                            {
                                result[role.Name]++;
                            }
                            else
                            {
                                result[role.Name] = 1;
                            }
                        }
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el número de sesiones activas por rol");
                throw;
            }
        }

        /// <summary>
        /// Obtiene el número de nuevas sesiones en las últimas horas
        /// </summary>
        /// <param name="hours">Número de horas hacia atrás</param>
        /// <returns>Número de nuevas sesiones</returns>
        public async Task<int> GetNewSessionsInLastHoursAsync(int hours)
        {
            try
            {
                var cutoffTime = DateTime.UtcNow.AddHours(-hours);
                return await _unitOfWork.UserSessions.CountAsync(s => s.CreatedAt >= cutoffTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el número de nuevas sesiones en las últimas {Hours} horas", hours);
                throw;
            }
        }

        /// <summary>
        /// Actualiza la actividad de una sesión
        /// </summary>
        /// <param name="sessionId">ID de la sesión</param>
        /// <returns>True si la sesión se actualizó correctamente, false en caso contrario</returns>
        public async Task<bool> UpdateSessionActivityAsync(Guid sessionId)
        {
            try
            {
                // Verificar si el seguimiento de actividad está habilitado
                if (!_trackUserActivity)
                {
                    return true;
                }
                
                var session = await _unitOfWork.UserSessions.GetByIdAsync(sessionId);
                if (session == null || !session.IsActive || session.EndedAt != null)
                {
                    return false;
                }
                
                // Verificar si ha pasado suficiente tiempo desde la última actualización
                var timeSinceLastUpdate = DateTime.UtcNow - session.LastActivity;
                if (timeSinceLastUpdate.TotalSeconds < _activityTrackingIntervalSeconds)
                {
                    return true;
                }
                
                // Actualizar la fecha de última actividad
                session.LastActivity = DateTime.UtcNow;
                await _unitOfWork.UserSessions.UpdateAsync(session);
                await _unitOfWork.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar la actividad de la sesión {SessionId}", sessionId);
                return false;
            }
        }

        /// <summary>
        /// Cierra una sesión
        /// </summary>
        /// <param name="sessionId">ID de la sesión</param>
        /// <param name="reason">Motivo del cierre</param>
        /// <returns>True si la sesión se cerró correctamente, false en caso contrario</returns>
        public async Task<bool> CloseSessionAsync(Guid sessionId, string reason)
        {
            try
            {
                var session = await _unitOfWork.UserSessions.GetByIdAsync(sessionId);
                if (session == null || !session.IsActive || session.EndedAt != null)
                {
                    return false;
                }
                
                // Marcar la sesión como cerrada
                session.IsActive = false;
                session.EndedAt = DateTime.UtcNow;
                session.EndReason = reason;
                session.LastModifiedAt = DateTime.UtcNow;
                
                await _unitOfWork.UserSessions.UpdateAsync(session);
                
                // Revocar el token JWT
                if (!string.IsNullOrEmpty(session.AccessToken))
                {
                    await _tokenRevocationService.RevokeTokenAsync(
                        session.AccessToken,
                        session.UserId,
                        reason
                    );
                }
                
                await _unitOfWork.SaveChangesAsync();
                
                _logger.LogInformation("Sesión {SessionId} cerrada. Motivo: {Reason}", sessionId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar la sesión {SessionId}", sessionId);
                return false;
            }
        }

        /// <summary>
        /// Cierra todas las sesiones de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="reason">Motivo del cierre</param>
        /// <param name="excludeSessionId">ID de la sesión a excluir (opcional)</param>
        /// <returns>Número de sesiones cerradas</returns>
        public async Task<int> CloseAllUserSessionsAsync(Guid userId, string reason, Guid? excludeSessionId = null)
        {
            try
            {
                var activeSessions = await _unitOfWork.UserSessions.GetActiveByUserIdAsync(userId);
                int closedCount = 0;
                
                foreach (var session in activeSessions)
                {
                    // Excluir la sesión especificada
                    if (excludeSessionId.HasValue && session.Id == excludeSessionId.Value)
                    {
                        continue;
                    }
                    
                    if (await CloseSessionAsync(session.Id, reason))
                    {
                        closedCount++;
                    }
                }
                
                _logger.LogInformation("Se cerraron {ClosedCount} sesiones del usuario {UserId}. Motivo: {Reason}", closedCount, userId, reason);
                return closedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar todas las sesiones del usuario {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Cierra todas las sesiones excepto la actual
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="currentSessionId">ID de la sesión actual</param>
        /// <param name="reason">Motivo del cierre</param>
        /// <returns>Número de sesiones cerradas</returns>
        public async Task<int> CloseAllOtherSessionsAsync(Guid userId, Guid currentSessionId, string reason)
        {
            try
            {
                return await CloseAllUserSessionsAsync(userId, reason, currentSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar todas las sesiones excepto la actual del usuario {UserId}", userId);
                throw;
            }
        }

        /// <summary>
        /// Verifica si un usuario ha excedido el número máximo de sesiones concurrentes
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>True si el usuario ha excedido el límite, false en caso contrario</returns>
        public async Task<bool> HasExceededMaxConcurrentSessionsAsync(Guid userId)
        {
            try
            {
                var activeSessions = await _unitOfWork.UserSessions.GetActiveByUserIdAsync(userId);
                return activeSessions.Count >= _maxConcurrentSessions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar si el usuario {UserId} ha excedido el número máximo de sesiones concurrentes", userId);
                throw;
            }
        }

        /// <summary>
        /// Limpia las sesiones expiradas
        /// </summary>
        /// <returns>Número de sesiones eliminadas</returns>
        public async Task<int> CleanupExpiredSessionsAsync()
        {
            try
            {
                var now = DateTime.UtcNow;
                var expiredSessions = await _unitOfWork.UserSessions.GetAllAsync(s => s.IsActive && s.EndedAt == null && s.ExpiresAt < now);
                
                int cleanedCount = 0;
                foreach (var session in expiredSessions)
                {
                    if (await CloseSessionAsync(session.Id, "Sesión expirada"))
                    {
                        cleanedCount++;
                    }
                }
                
                _logger.LogInformation("Se limpiaron {CleanedCount} sesiones expiradas", cleanedCount);
                return cleanedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al limpiar sesiones expiradas");
                throw;
            }
        }
    }
}
