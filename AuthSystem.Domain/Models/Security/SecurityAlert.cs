using System;

namespace AuthSystem.Domain.Models.Security
{
    /// <summary>
    /// Modelo para una alerta de seguridad
    /// </summary>
    public class SecurityAlert
    {
        /// <summary>
        /// Identificador único de la alerta
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Tipo de alerta
        /// </summary>
        public SecurityAlertType Type { get; set; }

        /// <summary>
        /// Nivel de severidad de la alerta
        /// </summary>
        public SecurityAlertSeverity Severity { get; set; }

        /// <summary>
        /// Mensaje descriptivo de la alerta
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Detalles adicionales de la alerta
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Usuario relacionado con la alerta (si aplica)
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Dirección IP relacionada con la alerta (si aplica)
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Fecha y hora en que se generó la alerta
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica si la alerta ha sido revisada
        /// </summary>
        public bool IsReviewed { get; set; }

        /// <summary>
        /// Fecha y hora en que se revisó la alerta
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// Usuario que revisó la alerta
        /// </summary>
        public string ReviewedBy { get; set; }

        /// <summary>
        /// Notas adicionales sobre la alerta
        /// </summary>
        public string Notes { get; set; }
    }

    /// <summary>
    /// Tipos de alertas de seguridad
    /// </summary>
    public enum SecurityAlertType
    {
        /// <summary>
        /// Múltiples intentos fallidos de inicio de sesión
        /// </summary>
        MultipleFailedLogins,

        /// <summary>
        /// Intento de inicio de sesión desde una ubicación inusual
        /// </summary>
        UnusualLocation,

        /// <summary>
        /// Intento de inicio de sesión en un horario inusual
        /// </summary>
        UnusualTime,

        /// <summary>
        /// Múltiples cuentas atacadas desde la misma IP
        /// </summary>
        MultipleAccountsFromSameIp,

        /// <summary>
        /// Posible ataque de fuerza bruta
        /// </summary>
        PossibleBruteForce,

        /// <summary>
        /// Posible ataque de diccionario
        /// </summary>
        PossibleDictionaryAttack,

        /// <summary>
        /// Cuenta bloqueada
        /// </summary>
        AccountLocked,

        /// <summary>
        /// Intento de acceso a funcionalidad restringida
        /// </summary>
        UnauthorizedAccessAttempt,

        /// <summary>
        /// Otro tipo de alerta
        /// </summary>
        Other
    }

    /// <summary>
    /// Niveles de severidad de las alertas de seguridad
    /// </summary>
    public enum SecurityAlertSeverity
    {
        /// <summary>
        /// Baja severidad - Informativo
        /// </summary>
        Low,

        /// <summary>
        /// Severidad media - Requiere atención
        /// </summary>
        Medium,

        /// <summary>
        /// Alta severidad - Requiere acción inmediata
        /// </summary>
        High,

        /// <summary>
        /// Crítica - Posible brecha de seguridad
        /// </summary>
        Critical
    }
}
