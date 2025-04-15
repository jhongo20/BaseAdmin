using System;
using System.Collections.Generic;

namespace AuthSystem.Domain.Common.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando falla la validación de datos
    /// </summary>
    public class ValidationException : DomainException
    {
        /// <summary>
        /// Errores de validación
        /// </summary>
        public IDictionary<string, string[]> Errors { get; }

        /// <summary>
        /// Constructor con mensaje de error simple
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        public ValidationException(string message) : base(message)
        {
            Errors = new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Constructor con errores de validación
        /// </summary>
        /// <param name="errors">Diccionario de errores de validación</param>
        public ValidationException(IDictionary<string, string[]> errors) : base("Se han producido uno o más errores de validación.")
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }

        /// <summary>
        /// Constructor con mensaje de error y errores de validación
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        /// <param name="errors">Diccionario de errores de validación</param>
        public ValidationException(string message, IDictionary<string, string[]> errors) : base(message)
        {
            Errors = errors ?? new Dictionary<string, string[]>();
        }
    }
}
