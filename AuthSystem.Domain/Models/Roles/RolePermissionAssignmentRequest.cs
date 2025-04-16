using System;
using System.Collections.Generic;

namespace AuthSystem.Domain.Models.Roles
{
    /// <summary>
    /// Modelo para la asignación de permisos a un rol
    /// </summary>
    public class RolePermissionAssignmentRequest
    {
        /// <summary>
        /// Lista de IDs de permisos a asignar al rol
        /// </summary>
        public List<Guid> PermissionIds { get; set; } = new List<Guid>();
    }
}
