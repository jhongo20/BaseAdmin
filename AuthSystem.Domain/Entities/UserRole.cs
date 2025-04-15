using System;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad de relación entre usuarios y roles
    /// </summary>
    public class UserRole : BaseEntity
    {
        /// <summary>
        /// Usuario al que se asigna el rol
        /// </summary>
        public User User { get; set; }
        
        /// <summary>
        /// ID del usuario
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Rol asignado al usuario
        /// </summary>
        public Role Role { get; set; }
        
        /// <summary>
        /// ID del rol
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Fecha de inicio de validez del rol (opcional)
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// Fecha de fin de validez del rol (opcional)
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// Verifica si el rol está actualmente vigente
        /// </summary>
        /// <returns>True si el rol está vigente</returns>
        public bool IsValid()
        {
            var now = DateTime.UtcNow;
            return IsActive && 
                   (!ValidFrom.HasValue || ValidFrom.Value <= now) && 
                   (!ValidTo.HasValue || ValidTo.Value >= now);
        }
    }
}
