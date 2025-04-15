using System;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad de relación entre usuarios y sucursales
    /// </summary>
    public class UserBranch : BaseEntity
    {
        /// <summary>
        /// Usuario asignado a la sucursal
        /// </summary>
        public User User { get; set; }
        
        /// <summary>
        /// ID del usuario
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Sucursal a la que está asignado el usuario
        /// </summary>
        public Branch Branch { get; set; }
        
        /// <summary>
        /// ID de la sucursal
        /// </summary>
        public Guid BranchId { get; set; }

        /// <summary>
        /// Indica si es la sucursal principal del usuario
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Fecha de inicio de asignación a la sucursal
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// Fecha de fin de asignación a la sucursal
        /// </summary>
        public DateTime? ValidTo { get; set; }

        /// <summary>
        /// Verifica si la asignación a la sucursal está vigente
        /// </summary>
        /// <returns>True si la asignación está vigente</returns>
        public bool IsValid()
        {
            var now = DateTime.UtcNow;
            return IsActive && 
                   (!ValidFrom.HasValue || ValidFrom.Value <= now) && 
                   (!ValidTo.HasValue || ValidTo.Value >= now);
        }
    }
}
