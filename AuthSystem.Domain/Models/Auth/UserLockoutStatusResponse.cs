using System;

namespace AuthSystem.Domain.Models.Auth
{
    /// <summary>
    /// Modelo de respuesta para el estado de bloqueo de un usuario
    /// </summary>
    public class UserLockoutStatusResponse
    {
        /// <summary>
        /// Identificador del usuario
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Estado del usuario (Active, Inactive, Locked, etc.)
        /// </summary>
        public string Status { get; set; }

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

        /// <summary>
        /// Fecha de último inicio de sesión
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }
}
