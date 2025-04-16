using System;
using System.Collections.Generic;

namespace AuthSystem.Domain.Models.Security
{
    /// <summary>
    /// Modelo para estadísticas de seguridad
    /// </summary>
    public class SecurityStats
    {
        /// <summary>
        /// Número total de intentos fallidos de inicio de sesión en las últimas 24 horas
        /// </summary>
        public int FailedLoginAttempts24h { get; set; }

        /// <summary>
        /// Número total de cuentas bloqueadas en las últimas 24 horas
        /// </summary>
        public int AccountsLocked24h { get; set; }

        /// <summary>
        /// Número total de alertas generadas en las últimas 24 horas
        /// </summary>
        public int AlertsGenerated24h { get; set; }

        /// <summary>
        /// Número de alertas por severidad
        /// </summary>
        public Dictionary<SecurityAlertSeverity, int> AlertsBySeverity { get; set; } = new Dictionary<SecurityAlertSeverity, int>();

        /// <summary>
        /// Número de alertas por tipo
        /// </summary>
        public Dictionary<SecurityAlertType, int> AlertsByType { get; set; } = new Dictionary<SecurityAlertType, int>();

        /// <summary>
        /// Top 5 de direcciones IP con más intentos fallidos
        /// </summary>
        public List<IpStats> TopFailedIpAddresses { get; set; } = new List<IpStats>();

        /// <summary>
        /// Top 5 de usuarios con más intentos fallidos
        /// </summary>
        public List<UserStats> TopFailedUsers { get; set; } = new List<UserStats>();

        /// <summary>
        /// Fecha y hora de la última alerta crítica
        /// </summary>
        public DateTime? LastCriticalAlertTime { get; set; }

        /// <summary>
        /// Número de intentos fallidos por hora (últimas 24 horas)
        /// </summary>
        public Dictionary<int, int> FailedAttemptsByHour { get; set; } = new Dictionary<int, int>();
    }

    /// <summary>
    /// Estadísticas por dirección IP
    /// </summary>
    public class IpStats
    {
        /// <summary>
        /// Dirección IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Número de intentos
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Ubicación geográfica aproximada
        /// </summary>
        public string Location { get; set; }
    }

    /// <summary>
    /// Estadísticas por usuario
    /// </summary>
    public class UserStats
    {
        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Número de intentos
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Indica si la cuenta está bloqueada
        /// </summary>
        public bool IsLocked { get; set; }
    }
}
