using System;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa una sesión activa de usuario
    /// </summary>
    public class UserSession : BaseEntity
    {
        /// <summary>
        /// Usuario al que pertenece la sesión
        /// </summary>
        public User User { get; set; }
        
        /// <summary>
        /// ID del usuario
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Token JWT emitido para esta sesión
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Identificador único del token JWT (jti claim)
        /// </summary>
        public string TokenId { get; set; }

        /// <summary>
        /// Token de actualización (refresh token)
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// Fecha y hora de expiración del token
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Dirección IP desde la que se inició la sesión
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Información del dispositivo/navegador
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Token de acceso (JWT completo)
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Fecha y hora de finalización de la sesión
        /// </summary>
        public DateTime? EndedAt { get; set; }

        /// <summary>
        /// Motivo de finalización de la sesión
        /// </summary>
        public string EndReason { get; set; }

        /// <summary>
        /// Fecha y hora del último acceso
        /// </summary>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// Indica si la sesión ha sido revocada manualmente
        /// </summary>
        public bool IsRevoked { get; set; }

        /// <summary>
        /// Razón por la que se revocó la sesión (si aplica)
        /// </summary>
        public string RevocationReason { get; set; }

        /// <summary>
        /// Verifica si la sesión está activa
        /// </summary>
        /// <returns>True si la sesión está activa</returns>
        public bool IsSessionActive()
        {
            return base.IsActive && !IsRevoked && ExpiresAt > DateTime.UtcNow;
        }

        /// <summary>
        /// Revoca la sesión
        /// </summary>
        /// <param name="reason">Razón de la revocación</param>
        public void Revoke(string reason)
        {
            IsRevoked = true;
            RevocationReason = reason;
            LastModifiedAt = DateTime.UtcNow;
        }
    }
}
