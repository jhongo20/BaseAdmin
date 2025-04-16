using System;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un intento de inicio de sesión
    /// </summary>
    public class UserLoginAttempt : BaseEntity
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Nombre de usuario utilizado en el intento
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Dirección IP desde la que se realizó el intento
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User-Agent desde el que se realizó el intento
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Resultado del intento (éxito o fracaso)
        /// </summary>
        public bool Successful { get; set; }

        /// <summary>
        /// Mensaje de error (si el intento falló)
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Usuario relacionado con el intento
        /// </summary>
        public User User { get; set; }
    }
}
