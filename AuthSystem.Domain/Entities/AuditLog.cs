using System;
using AuthSystem.Domain.Common.Enums;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que registra acciones de auditoría en el sistema
    /// </summary>
    public class AuditLog : BaseEntity
    {
        /// <summary>
        /// Tipo de acción realizada
        /// </summary>
        public AuditActionType ActionType { get; set; }

        /// <summary>
        /// Entidad sobre la que se realizó la acción
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Identificador de la entidad afectada
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// Datos anteriores (para acciones de actualización o eliminación)
        /// </summary>
        public string OldValues { get; set; }

        /// <summary>
        /// Nuevos datos (para acciones de creación o actualización)
        /// </summary>
        public string NewValues { get; set; }

        /// <summary>
        /// Usuario que realizó la acción
        /// </summary>
        public User User { get; set; }
        
        /// <summary>
        /// ID del usuario que realizó la acción
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Dirección IP desde donde se realizó la acción
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Información del dispositivo/navegador
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Módulo del sistema donde se realizó la acción
        /// </summary>
        public string ModuleName { get; set; }

        /// <summary>
        /// Endpoint o ruta donde se realizó la acción
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Organización en la que se realizó la acción
        /// </summary>
        public Organization Organization { get; set; }
        
        /// <summary>
        /// ID de la organización en la que se realizó la acción
        /// </summary>
        public Guid? OrganizationId { get; set; }

        /// <summary>
        /// Sucursal en la que se realizó la acción
        /// </summary>
        public Branch Branch { get; set; }
        
        /// <summary>
        /// ID de la sucursal en la que se realizó la acción
        /// </summary>
        public Guid? BranchId { get; set; }

        /// <summary>
        /// Información adicional relevante para la auditoría
        /// </summary>
        public string AdditionalInfo { get; set; }

        /// <summary>
        /// Cadena de consulta de la solicitud HTTP
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// Datos de la solicitud HTTP
        /// </summary>
        public string RequestData { get; set; }

        /// <summary>
        /// Datos de la respuesta HTTP
        /// </summary>
        public string ResponseData { get; set; }

        /// <summary>
        /// Código de estado HTTP
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Marca de tiempo de la acción
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Nivel de severidad del evento (informativo, advertencia, error)
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// Indica si la acción fue exitosa
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Mensaje de error (si la acción no fue exitosa)
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
