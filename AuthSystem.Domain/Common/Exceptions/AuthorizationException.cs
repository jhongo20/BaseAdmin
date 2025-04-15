using System;

namespace AuthSystem.Domain.Common.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando un usuario no tiene permisos para realizar una acción
    /// </summary>
    public class AuthorizationException : DomainException
    {
        /// <summary>
        /// Constructor con mensaje de error
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        public AuthorizationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor con mensaje de error y excepción interna
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        /// <param name="innerException">Excepción interna</param>
        public AuthorizationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Constructor con usuario y recurso
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="resource">Recurso al que se intenta acceder</param>
        public AuthorizationException(string userId, string resource) 
            : base($"Usuario {userId} no tiene permiso para acceder al recurso {resource}.")
        {
        }
    }
}
