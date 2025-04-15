using System;
using System.Collections.Generic;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un módulo funcional del sistema
    /// </summary>
    public class Module : BaseEntity
    {
        /// <summary>
        /// Nombre único del módulo
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del módulo
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ruta base del módulo en la aplicación
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Icono asociado al módulo
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Orden de visualización del módulo
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Indica si el módulo está habilitado
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Módulo padre (para módulos anidados)
        /// </summary>
        public Module Parent { get; set; }
        
        /// <summary>
        /// ID del módulo padre
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// Relación con los módulos hijos
        /// </summary>
        public virtual ICollection<Module> Children { get; set; } = new List<Module>();

        /// <summary>
        /// Relación con los permisos asociados a este módulo
        /// </summary>
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
