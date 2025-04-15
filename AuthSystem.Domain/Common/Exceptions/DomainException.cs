using System;

namespace AuthSystem.Domain.Common.Exceptions
{
    /// <summary>
    /// Excepción base para todas las excepciones del dominio
    /// </summary>
    public abstract class DomainException : Exception
    {
        /// <summary>
        /// Constructor con mensaje de error
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        protected DomainException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor con mensaje de error y excepción interna
        /// </summary>
        /// <param name="message">Mensaje de error</param>
        /// <param name="innerException">Excepción interna</param>
        protected DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
