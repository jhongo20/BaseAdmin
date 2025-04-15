using System;
using System.Collections.Generic;
using AuthSystem.Domain.Common.Enums;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un permiso en el sistema
    /// </summary>
    public class Permission : BaseEntity
    {
        /// <summary>
        /// Nombre único del permiso
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del permiso
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Tipo de permiso (lectura, escritura, etc.)
        /// </summary>
        public PermissionType Type { get; set; }

        /// <summary>
        /// Módulo al que pertenece este permiso
        /// </summary>
        public Module Module { get; set; }
        
        /// <summary>
        /// ID del módulo al que pertenece este permiso
        /// </summary>
        public Guid ModuleId { get; set; }

        /// <summary>
        /// Relación con los roles que tienen este permiso
        /// </summary>
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
