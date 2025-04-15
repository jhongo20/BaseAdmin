using System;

namespace AuthSystem.Domain.Common.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando ocurre un error de autenticación
    /// </summary>
    public class AuthenticationException : DomainException
    {
        /// <summary>
        /// Constructor con mensaje de error
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        public AuthenticationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor con mensaje de error y excepción interna
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        /// <param name="innerException">Excepción interna</param>
        public AuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
