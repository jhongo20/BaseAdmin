using System;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad de relación entre roles y permisos
    /// </summary>
    public class RolePermission : BaseEntity
    {
        /// <summary>
        /// Rol al que se asigna el permiso
        /// </summary>
        public Role Role { get; set; }
        
        /// <summary>
        /// ID del rol
        /// </summary>
        public Guid RoleId { get; set; }

        /// <summary>
        /// Permiso asignado al rol
        /// </summary>
        public Permission Permission { get; set; }
        
        /// <summary>
        /// ID del permiso
        /// </summary>
        public Guid PermissionId { get; set; }

        /// <summary>
        /// Fecha de inicio de validez del permiso (opcional)
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// Fecha de fin de validez del permiso (opcional)
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// Verifica si el permiso está actualmente vigente
        /// </summary>
        /// <returns>True si el permiso está vigente</returns>
        public bool IsValid()
        {
            var now = DateTime.UtcNow;
            return IsActive && 
                   (!ValidFrom.HasValue || ValidFrom.Value <= now) && 
                   (!ValidTo.HasValue || ValidTo.Value >= now);
        }
    }
}
