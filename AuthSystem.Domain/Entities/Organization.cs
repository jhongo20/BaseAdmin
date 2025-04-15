using System.Collections.Generic;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa una organización en el sistema
    /// </summary>
    public class Organization : BaseEntity
    {
        /// <summary>
        /// Nombre de la organización
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción de la organización
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Identificador fiscal de la organización (NIT, RFC, etc.)
        /// </summary>
        public string TaxId { get; set; }

        /// <summary>
        /// Dirección de la sede principal
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Teléfono de contacto
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Correo electrónico de contacto
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Sitio web de la organización
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        /// Logo de la organización (URL o ruta)
        /// </summary>
        public string LogoUrl { get; set; }

        /// <summary>
        /// Configuración de conexión LDAP (para organizaciones con usuarios internos)
        /// </summary>
        public string LdapConfig { get; set; }

        /// <summary>
        /// Relación con las sucursales de esta organización
        /// </summary>
        public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();

        /// <summary>
        /// Relación con los roles específicos de esta organización
        /// </summary>
        public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}
