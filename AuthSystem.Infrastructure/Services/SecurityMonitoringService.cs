using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de monitoreo de seguridad
    /// </summary>
    public class SecurityMonitoringService : ISecurityMonitoringService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SecurityMonitoringService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        
        // En una implementación real, estos datos deberían almacenarse en una base de datos
        private static readonly ConcurrentDictionary<string, List<FailedLoginAttempt>> _failedLoginAttempts = 
            new ConcurrentDictionary<string, List<FailedLoginAttempt>>();
        
        private static readonly ConcurrentDictionary<string, List<SuccessfulLogin>> _successfulLogins = 
            new ConcurrentDictionary<string, List<SuccessfulLogin>>();
        
        private static readonly ConcurrentDictionary<string, AccountLockout> _accountLockouts = 
            new ConcurrentDictionary<string, AccountLockout>();
        
        private static readonly List<SecurityAlert> _securityAlerts = new List<SecurityAlert>();

        // Umbrales de detección
        private readonly int _failedLoginThreshold;
        private readonly int _failedLoginTimeWindowMinutes;
        private readonly int _multipleAccountsThreshold;
        private readonly bool _enableAlertNotifications;

        public SecurityMonitoringService(
            IUnitOfWork unitOfWork,
            ILogger<SecurityMonitoringService> logger,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;
            
            // Cargar configuración
            _failedLoginThreshold = _configuration.GetValue<int>("SecurityMonitoring:FailedLoginThreshold", 5);
            _failedLoginTimeWindowMinutes = _configuration.GetValue<int>("SecurityMonitoring:FailedLoginTimeWindowMinutes", 10);
            _multipleAccountsThreshold = _configuration.GetValue<int>("SecurityMonitoring:MultipleAccountsThreshold", 3);
            _enableAlertNotifications = _configuration.GetValue<bool>("SecurityMonitoring:EnableAlertNotifications", true);
            
            // Iniciar la tarea de detección periódica
            StartPeriodicDetection();
        }

        /// <inheritdoc />
        public async Task RecordFailedLoginAttemptAsync(string username, string ipAddress, string userAgent, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(ipAddress))
                return;

            try
            {
                var attempt = new FailedLoginAttempt
                {
                    Username = username,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = timestamp
                };

                // Registrar por usuario
                _failedLoginAttempts.AddOrUpdate(
                    username,
                    new List<FailedLoginAttempt> { attempt },
                    (key, list) =>
                    {
                        list.Add(attempt);
                        // Mantener solo los intentos de las últimas 24 horas
                        return list.Where(a => a.Timestamp > DateTime.UtcNow.AddHours(-24)).ToList();
                    });

                // Registrar por IP
                _failedLoginAttempts.AddOrUpdate(
                    $"ip:{ipAddress}",
                    new List<FailedLoginAttempt> { attempt },
                    (key, list) =>
                    {
                        list.Add(attempt);
                        // Mantener solo los intentos de las últimas 24 horas
                        return list.Where(a => a.Timestamp > DateTime.UtcNow.AddHours(-24)).ToList();
                    });

                _logger.LogInformation("Intento fallido de inicio de sesión registrado: Usuario={Username}, IP={IpAddress}", username, ipAddress);

                // Detectar patrones sospechosos inmediatamente
                await DetectSuspiciousPatternsForUserAsync(username, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar intento fallido de inicio de sesión: Usuario={Username}, IP={IpAddress}", username, ipAddress);
            }
        }

        /// <inheritdoc />
        public Task RecordSuccessfulLoginAsync(string username, string ipAddress, string userAgent, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(ipAddress))
                return Task.CompletedTask;

            try
            {
                var login = new SuccessfulLogin
                {
                    Username = username,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    Timestamp = timestamp
                };

                _successfulLogins.AddOrUpdate(
                    username,
                    new List<SuccessfulLogin> { login },
                    (key, list) =>
                    {
                        list.Add(login);
                        // Mantener solo los inicios de sesión de las últimas 24 horas
                        return list.Where(l => l.Timestamp > DateTime.UtcNow.AddHours(-24)).ToList();
                    });

                _logger.LogInformation("Inicio de sesión exitoso registrado: Usuario={Username}, IP={IpAddress}", username, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar inicio de sesión exitoso: Usuario={Username}, IP={IpAddress}", username, ipAddress);
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task RecordAccountLockoutAsync(string username, string reason, int duration, DateTime timestamp)
        {
            if (string.IsNullOrEmpty(username))
                return;

            try
            {
                var lockout = new AccountLockout
                {
                    Username = username,
                    Reason = reason,
                    Duration = duration,
                    Timestamp = timestamp,
                    UnlockTime = timestamp.AddMinutes(duration)
                };

                _accountLockouts[username] = lockout;

                _logger.LogWarning("Cuenta bloqueada: Usuario={Username}, Razón={Reason}, Duración={Duration} minutos", 
                    username, reason, duration);

                // Crear alerta de seguridad
                var alert = new SecurityAlert
                {
                    Type = SecurityAlertType.AccountLocked,
                    Severity = SecurityAlertSeverity.Medium,
                    Message = $"Cuenta bloqueada: {username}",
                    Details = $"La cuenta del usuario {username} ha sido bloqueada por {duration} minutos. Razón: {reason}",
                    Username = username,
                    Timestamp = timestamp
                };

                lock (_securityAlerts)
                {
                    _securityAlerts.Add(alert);
                }

                // Enviar notificación si está habilitado
                if (_enableAlertNotifications)
                {
                    await NotifySecurityTeamAsync(alert);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar bloqueo de cuenta: Usuario={Username}", username);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SecurityAlert>> DetectSuspiciousPatterns()
        {
            var alerts = new List<SecurityAlert>();

            try
            {
                // Detectar múltiples intentos fallidos por usuario
                foreach (var kvp in _failedLoginAttempts)
                {
                    if (kvp.Key.StartsWith("ip:"))
                        continue;

                    string username = kvp.Key;
                    var attempts = kvp.Value;

                    // Filtrar intentos en la ventana de tiempo configurada
                    var recentAttempts = attempts
                        .Where(a => a.Timestamp > DateTime.UtcNow.AddMinutes(-_failedLoginTimeWindowMinutes))
                        .ToList();

                    if (recentAttempts.Count >= _failedLoginThreshold)
                    {
                        var alert = new SecurityAlert
                        {
                            Type = SecurityAlertType.MultipleFailedLogins,
                            Severity = SecurityAlertSeverity.Medium,
                            Message = $"Múltiples intentos fallidos de inicio de sesión: {username}",
                            Details = $"Se detectaron {recentAttempts.Count} intentos fallidos de inicio de sesión para el usuario {username} en los últimos {_failedLoginTimeWindowMinutes} minutos.",
                            Username = username,
                            IpAddress = recentAttempts.Last().IpAddress,
                            Timestamp = DateTime.UtcNow
                        };

                        alerts.Add(alert);
                    }
                }

                // Detectar múltiples cuentas atacadas desde la misma IP
                foreach (var kvp in _failedLoginAttempts.Where(k => k.Key.StartsWith("ip:")))
                {
                    string ipAddress = kvp.Key.Substring(3); // Quitar el prefijo "ip:"
                    var attempts = kvp.Value;

                    // Filtrar intentos en la ventana de tiempo configurada
                    var recentAttempts = attempts
                        .Where(a => a.Timestamp > DateTime.UtcNow.AddMinutes(-_failedLoginTimeWindowMinutes))
                        .ToList();

                    // Contar usuarios únicos
                    var uniqueUsers = recentAttempts.Select(a => a.Username).Distinct().ToList();

                    if (uniqueUsers.Count >= _multipleAccountsThreshold)
                    {
                        var alert = new SecurityAlert
                        {
                            Type = SecurityAlertType.MultipleAccountsFromSameIp,
                            Severity = SecurityAlertSeverity.High,
                            Message = $"Múltiples cuentas atacadas desde la misma IP: {ipAddress}",
                            Details = $"Se detectaron intentos fallidos de inicio de sesión para {uniqueUsers.Count} cuentas diferentes desde la IP {ipAddress} en los últimos {_failedLoginTimeWindowMinutes} minutos.",
                            IpAddress = ipAddress,
                            Timestamp = DateTime.UtcNow
                        };

                        alerts.Add(alert);
                    }
                }

                // Detectar posibles ataques de fuerza bruta
                foreach (var kvp in _failedLoginAttempts.Where(k => k.Key.StartsWith("ip:")))
                {
                    string ipAddress = kvp.Key.Substring(3);
                    var attempts = kvp.Value;

                    // Filtrar intentos en los últimos 5 minutos
                    var recentAttempts = attempts
                        .Where(a => a.Timestamp > DateTime.UtcNow.AddMinutes(-5))
                        .ToList();

                    // Si hay más de 10 intentos en 5 minutos, podría ser un ataque de fuerza bruta
                    if (recentAttempts.Count >= 10)
                    {
                        var alert = new SecurityAlert
                        {
                            Type = SecurityAlertType.PossibleBruteForce,
                            Severity = SecurityAlertSeverity.Critical,
                            Message = $"Posible ataque de fuerza bruta desde IP: {ipAddress}",
                            Details = $"Se detectaron {recentAttempts.Count} intentos fallidos de inicio de sesión desde la IP {ipAddress} en los últimos 5 minutos.",
                            IpAddress = ipAddress,
                            Timestamp = DateTime.UtcNow
                        };

                        alerts.Add(alert);
                    }
                }

                // Almacenar las alertas generadas
                if (alerts.Any())
                {
                    lock (_securityAlerts)
                    {
                        _securityAlerts.AddRange(alerts);
                        
                        // Mantener solo las alertas de los últimos 30 días
                        var cutoffDate = DateTime.UtcNow.AddDays(-30);
                        _securityAlerts.RemoveAll(a => a.Timestamp < cutoffDate);
                    }

                    // Enviar notificaciones para alertas críticas
                    if (_enableAlertNotifications)
                    {
                        foreach (var alert in alerts.Where(a => a.Severity == SecurityAlertSeverity.Critical))
                        {
                            await NotifySecurityTeamAsync(alert);
                        }
                    }

                    _logger.LogWarning("Se generaron {Count} alertas de seguridad", alerts.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al detectar patrones sospechosos");
            }

            return alerts;
        }

        /// <inheritdoc />
        public Task<IEnumerable<SecurityAlert>> GetRecentAlertsAsync(int count = 10)
        {
            try
            {
                IEnumerable<SecurityAlert> alerts;
                
                lock (_securityAlerts)
                {
                    alerts = _securityAlerts
                        .OrderByDescending(a => a.Timestamp)
                        .Take(count)
                        .ToList();
                }

                return Task.FromResult(alerts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener alertas recientes");
                return Task.FromResult<IEnumerable<SecurityAlert>>(new List<SecurityAlert>());
            }
        }

        /// <inheritdoc />
        public Task<SecurityStats> GetSecurityStatsAsync()
        {
            try
            {
                var stats = new SecurityStats();
                var cutoffTime = DateTime.UtcNow.AddHours(-24);

                // Contar intentos fallidos en las últimas 24 horas
                stats.FailedLoginAttempts24h = _failedLoginAttempts
                    .Where(kvp => !kvp.Key.StartsWith("ip:"))
                    .SelectMany(kvp => kvp.Value)
                    .Count(a => a.Timestamp > cutoffTime);

                // Contar cuentas bloqueadas en las últimas 24 horas
                stats.AccountsLocked24h = _accountLockouts
                    .Count(kvp => kvp.Value.Timestamp > cutoffTime);

                // Contar alertas generadas en las últimas 24 horas
                lock (_securityAlerts)
                {
                    stats.AlertsGenerated24h = _securityAlerts.Count(a => a.Timestamp > cutoffTime);

                    // Contar alertas por severidad
                    foreach (SecurityAlertSeverity severity in Enum.GetValues(typeof(SecurityAlertSeverity)))
                    {
                        stats.AlertsBySeverity[severity] = _securityAlerts
                            .Count(a => a.Timestamp > cutoffTime && a.Severity == severity);
                    }

                    // Contar alertas por tipo
                    foreach (SecurityAlertType type in Enum.GetValues(typeof(SecurityAlertType)))
                    {
                        stats.AlertsByType[type] = _securityAlerts
                            .Count(a => a.Timestamp > cutoffTime && a.Type == type);
                    }

                    // Obtener la fecha de la última alerta crítica
                    var lastCriticalAlert = _securityAlerts
                        .Where(a => a.Severity == SecurityAlertSeverity.Critical)
                        .OrderByDescending(a => a.Timestamp)
                        .FirstOrDefault();

                    if (lastCriticalAlert != null)
                    {
                        stats.LastCriticalAlertTime = lastCriticalAlert.Timestamp;
                    }
                }

                // Top 5 de direcciones IP con más intentos fallidos
                var ipStats = _failedLoginAttempts
                    .Where(kvp => kvp.Key.StartsWith("ip:"))
                    .Select(kvp => new
                    {
                        IpAddress = kvp.Key.Substring(3),
                        Count = kvp.Value.Count(a => a.Timestamp > cutoffTime)
                    })
                    .Where(x => x.Count > 0)
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();

                stats.TopFailedIpAddresses = ipStats.Select(x => new IpStats
                {
                    IpAddress = x.IpAddress,
                    Count = x.Count,
                    Location = "Desconocida" // En una implementación real, se podría usar un servicio de geolocalización
                }).ToList();

                // Top 5 de usuarios con más intentos fallidos
                var userStats = _failedLoginAttempts
                    .Where(kvp => !kvp.Key.StartsWith("ip:"))
                    .Select(kvp => new
                    {
                        Username = kvp.Key,
                        Count = kvp.Value.Count(a => a.Timestamp > cutoffTime),
                        IsLocked = _accountLockouts.ContainsKey(kvp.Key) && 
                                  _accountLockouts[kvp.Key].UnlockTime > DateTime.UtcNow
                    })
                    .Where(x => x.Count > 0)
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();

                stats.TopFailedUsers = userStats.Select(x => new UserStats
                {
                    Username = x.Username,
                    Count = x.Count,
                    IsLocked = x.IsLocked
                }).ToList();

                // Intentos fallidos por hora (últimas 24 horas)
                for (int i = 0; i < 24; i++)
                {
                    var hourStart = DateTime.UtcNow.AddHours(-i - 1);
                    var hourEnd = DateTime.UtcNow.AddHours(-i);

                    var count = _failedLoginAttempts
                        .Where(kvp => !kvp.Key.StartsWith("ip:"))
                        .SelectMany(kvp => kvp.Value)
                        .Count(a => a.Timestamp > hourStart && a.Timestamp <= hourEnd);

                    stats.FailedAttemptsByHour[i] = count;
                }

                return Task.FromResult(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de seguridad");
                return Task.FromResult(new SecurityStats());
            }
        }

        #region Métodos privados

        /// <summary>
        /// Inicia la detección periódica de patrones sospechosos
        /// </summary>
        private void StartPeriodicDetection()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await DetectSuspiciousPatterns();
                        await Task.Delay(TimeSpan.FromMinutes(5));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error en la detección periódica de patrones sospechosos");
                        await Task.Delay(TimeSpan.FromMinutes(1));
                    }
                }
            });
        }

        /// <summary>
        /// Detecta patrones sospechosos para un usuario específico
        /// </summary>
        /// <param name="username">Nombre de usuario</param>
        /// <param name="ipAddress">Dirección IP</param>
        private async Task DetectSuspiciousPatternsForUserAsync(string username, string ipAddress)
        {
            try
            {
                if (_failedLoginAttempts.TryGetValue(username, out var attempts))
                {
                    // Filtrar intentos en la ventana de tiempo configurada
                    var recentAttempts = attempts
                        .Where(a => a.Timestamp > DateTime.UtcNow.AddMinutes(-_failedLoginTimeWindowMinutes))
                        .ToList();

                    if (recentAttempts.Count >= _failedLoginThreshold)
                    {
                        var alert = new SecurityAlert
                        {
                            Type = SecurityAlertType.MultipleFailedLogins,
                            Severity = SecurityAlertSeverity.Medium,
                            Message = $"Múltiples intentos fallidos de inicio de sesión: {username}",
                            Details = $"Se detectaron {recentAttempts.Count} intentos fallidos de inicio de sesión para el usuario {username} en los últimos {_failedLoginTimeWindowMinutes} minutos.",
                            Username = username,
                            IpAddress = ipAddress,
                            Timestamp = DateTime.UtcNow
                        };

                        lock (_securityAlerts)
                        {
                            _securityAlerts.Add(alert);
                        }

                        if (_enableAlertNotifications)
                        {
                            await NotifySecurityTeamAsync(alert);
                        }

                        _logger.LogWarning("Alerta de seguridad generada: {Message}", alert.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al detectar patrones sospechosos para el usuario {Username}", username);
            }
        }

        /// <summary>
        /// Notifica al equipo de seguridad sobre una alerta
        /// </summary>
        /// <param name="alert">Alerta de seguridad</param>
        private async Task NotifySecurityTeamAsync(SecurityAlert alert)
        {
            try
            {
                string securityTeamEmail = _configuration.GetValue<string>("SecurityMonitoring:SecurityTeamEmail", "security@example.com");
                
                string subject = $"Alerta de Seguridad: {alert.Severity} - {alert.Type}";
                
                string body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #f8f9fa; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .footer {{ background-color: #f8f9fa; padding: 10px; text-align: center; font-size: 12px; }}
                        h1 {{ color: {GetSeverityColor(alert.Severity)}; }}
                        .info {{ background-color: #f8f9fa; padding: 15px; margin: 20px 0; border-left: 4px solid {GetSeverityColor(alert.Severity)}; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Alerta de Seguridad: {alert.Severity}</h1>
                        </div>
                        <div class='content'>
                            <p><strong>Tipo:</strong> {alert.Type}</p>
                            <p><strong>Mensaje:</strong> {alert.Message}</p>
                            
                            <div class='info'>
                                <p><strong>Detalles:</strong></p>
                                <p>{alert.Details}</p>
                                
                                <p><strong>Información adicional:</strong></p>
                                <ul>
                                    {(string.IsNullOrEmpty(alert.Username) ? "" : $"<li>Usuario: {alert.Username}</li>")}
                                    {(string.IsNullOrEmpty(alert.IpAddress) ? "" : $"<li>Dirección IP: {alert.IpAddress}</li>")}
                                    <li>Fecha y hora: {alert.Timestamp.ToString("dd/MM/yyyy HH:mm:ss")} UTC</li>
                                </ul>
                            </div>
                            
                            <p>Por favor, investigue esta alerta lo antes posible.</p>
                        </div>
                        <div class='footer'>
                            <p>Este es un mensaje automático del sistema de monitoreo de seguridad.</p>
                        </div>
                    </div>
                </body>
                </html>";

                await _emailService.SendEmailAsync(
                    securityTeamEmail,
                    subject,
                    body,
                    true);

                _logger.LogInformation("Notificación de alerta enviada al equipo de seguridad: {AlertType} - {AlertSeverity}", 
                    alert.Type, alert.Severity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al notificar al equipo de seguridad sobre una alerta");
            }
        }

        /// <summary>
        /// Obtiene el color correspondiente a una severidad
        /// </summary>
        /// <param name="severity">Severidad</param>
        /// <returns>Código de color HTML</returns>
        private string GetSeverityColor(SecurityAlertSeverity severity)
        {
            return severity switch
            {
                SecurityAlertSeverity.Low => "#28a745",
                SecurityAlertSeverity.Medium => "#ffc107",
                SecurityAlertSeverity.High => "#fd7e14",
                SecurityAlertSeverity.Critical => "#dc3545",
                _ => "#6c757d"
            };
        }

        #endregion

        #region Clases internas

        /// <summary>
        /// Clase interna para almacenar información de intentos fallidos de inicio de sesión
        /// </summary>
        private class FailedLoginAttempt
        {
            public string Username { get; set; }
            public string IpAddress { get; set; }
            public string UserAgent { get; set; }
            public DateTime Timestamp { get; set; }
        }

        /// <summary>
        /// Clase interna para almacenar información de inicios de sesión exitosos
        /// </summary>
        private class SuccessfulLogin
        {
            public string Username { get; set; }
            public string IpAddress { get; set; }
            public string UserAgent { get; set; }
            public DateTime Timestamp { get; set; }
        }

        /// <summary>
        /// Clase interna para almacenar información de bloqueos de cuentas
        /// </summary>
        private class AccountLockout
        {
            public string Username { get; set; }
            public string Reason { get; set; }
            public int Duration { get; set; }
            public DateTime Timestamp { get; set; }
            public DateTime UnlockTime { get; set; }
        }

        #endregion
    }
}
