using System;

namespace AuthSystem.Domain.Models.Auth
{
    /// <summary>
    /// Modelo de respuesta para el estado de bloqueo de una cuenta
    /// </summary>
    public class AccountLockoutStatusResponse
    {
        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Indica si la cuenta está bloqueada
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Número de intentos fallidos de inicio de sesión
        /// </summary>
        public int AccessFailedCount { get; set; }

        /// <summary>
        /// Fecha hasta la que el usuario está bloqueado
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// Minutos restantes para que expire el bloqueo
        /// </summary>
        public double RemainingMinutes { get; set; }
    }
}
