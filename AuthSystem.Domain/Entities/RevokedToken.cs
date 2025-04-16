using System;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un token JWT revocado
    /// </summary>
    public class RevokedToken : BaseEntity
    {
        /// <summary>
        /// Token JWT revocado (puede ser el token completo o su hash)
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Identificador único del token (JTI - JWT ID)
        /// </summary>
        public string TokenId { get; set; }

        /// <summary>
        /// Fecha de expiración del token
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Motivo de la revocación
        /// </summary>
        public string RevocationReason { get; set; }

        /// <summary>
        /// Usuario que revocó el token (puede ser null si fue revocado por el sistema)
        /// </summary>
        public Guid? RevokedByUserId { get; set; }

        /// <summary>
        /// Usuario al que pertenecía el token
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Usuario al que pertenecía el token
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Dirección IP desde la que se revocó el token
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User-Agent desde el que se revocó el token
        /// </summary>
        public string UserAgent { get; set; }
    }
}
