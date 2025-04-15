using System;

namespace AuthSystem.Domain.Common.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando no se encuentra un recurso solicitado
    /// </summary>
    public class NotFoundException : DomainException
    {
        /// <summary>
        /// Constructor con mensaje de error
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        public NotFoundException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor con nombre de entidad e identificador
        /// </summary>
        /// <param name="name">Nombre de la entidad</param>
        /// <param name="key">Identificador de la entidad</param>
        public NotFoundException(string name, object key) : base($"Entidad \"{name}\" ({key}) no encontrada.")
        {
        }
    }
}
