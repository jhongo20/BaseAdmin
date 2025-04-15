using System.Collections.Generic;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un rol en el sistema
    /// </summary>
    public class Role : BaseEntity
    {
        /// <summary>
        /// Nombre único del rol
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indica si es un rol de sistema (no modificable)
        /// </summary>
        public bool IsSystemRole { get; set; }

        /// <summary>
        /// Organización a la que pertenece el rol (null para roles globales)
        /// </summary>
        public Organization Organization { get; set; }
        
        /// <summary>
        /// ID de la organización a la que pertenece el rol
        /// </summary>
        public System.Guid? OrganizationId { get; set; }

        /// <summary>
        /// Relación con los usuarios asignados a este rol
        /// </summary>
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Relación con los permisos asignados a este rol
        /// </summary>
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
