using System;
using System.Collections.Generic;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa una sucursal o sede de una organización
    /// </summary>
    public class Branch : BaseEntity
    {
        /// <summary>
        /// Nombre de la sucursal
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Código único de la sucursal
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Descripción de la sucursal
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Dirección física de la sucursal
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Ciudad donde se encuentra la sucursal
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Estado o provincia donde se encuentra la sucursal
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// País donde se encuentra la sucursal
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Código postal de la sucursal
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Teléfono de contacto de la sucursal
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Correo electrónico de contacto de la sucursal
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Indica si es la sucursal principal de la organización
        /// </summary>
        public bool IsHeadquarters { get; set; }

        /// <summary>
        /// Organización a la que pertenece esta sucursal
        /// </summary>
        public Organization Organization { get; set; }
        
        /// <summary>
        /// ID de la organización a la que pertenece esta sucursal
        /// </summary>
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// Relación con los usuarios asignados a esta sucursal
        /// </summary>
        public virtual ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();
    }
}
