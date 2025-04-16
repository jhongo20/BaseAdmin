using System;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un token de restablecimiento de contraseña
    /// </summary>
    public class PasswordResetToken : BaseEntity
    {
        /// <summary>
        /// Token de restablecimiento
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// ID del usuario al que pertenece el token
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Usuario al que pertenece el token
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Fecha de expiración del token
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Indica si el token ha sido utilizado
        /// </summary>
        public bool IsUsed { get; set; }

        /// <summary>
        /// Fecha en la que se utilizó el token
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// Dirección IP desde la que se solicitó el token
        /// </summary>
        public string RequestIpAddress { get; set; }

        /// <summary>
        /// User-Agent desde el que se solicitó el token
        /// </summary>
        public string RequestUserAgent { get; set; }

        /// <summary>
        /// Dirección IP desde la que se utilizó el token
        /// </summary>
        public string UsedIpAddress { get; set; }

        /// <summary>
        /// User-Agent desde el que se utilizó el token
        /// </summary>
        public string UsedUserAgent { get; set; }
    }
}
