using System;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de registro (logging)
    /// </summary>
    public interface ILoggerService
    {
        /// <summary>
        /// Registra un mensaje de información
        /// </summary>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="args">Argumentos para formatear el mensaje</param>
        void Information(string message, params object[] args);

        /// <summary>
        /// Registra un mensaje de advertencia
        /// </summary>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="args">Argumentos para formatear el mensaje</param>
        void Warning(string message, params object[] args);

        /// <summary>
        /// Registra un mensaje de error
        /// </summary>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="args">Argumentos para formatear el mensaje</param>
        void Error(string message, params object[] args);

        /// <summary>
        /// Registra un mensaje de error con excepción
        /// </summary>
        /// <param name="exception">Excepción a registrar</param>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="args">Argumentos para formatear el mensaje</param>
        void Error(Exception exception, string message, params object[] args);

        /// <summary>
        /// Registra un mensaje de depuración
        /// </summary>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="args">Argumentos para formatear el mensaje</param>
        void Debug(string message, params object[] args);

        /// <summary>
        /// Registra un mensaje crítico
        /// </summary>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="args">Argumentos para formatear el mensaje</param>
        void Critical(string message, params object[] args);

        /// <summary>
        /// Registra un mensaje crítico con excepción
        /// </summary>
        /// <param name="exception">Excepción a registrar</param>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="args">Argumentos para formatear el mensaje</param>
        void Critical(Exception exception, string message, params object[] args);

        /// <summary>
        /// Registra un mensaje con propiedades estructuradas
        /// </summary>
        /// <param name="level">Nivel de registro (Information, Warning, Error, etc.)</param>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="properties">Propiedades estructuradas</param>
        void LogWithProperties(LogLevel level, string message, object properties);

        /// <summary>
        /// Registra un evento de seguridad
        /// </summary>
        /// <param name="eventId">ID del evento</param>
        /// <param name="message">Mensaje a registrar</param>
        /// <param name="properties">Propiedades estructuradas</param>
        void LogSecurityEvent(int eventId, string message, object properties);

        /// <summary>
        /// Registra un evento de rendimiento
        /// </summary>
        /// <param name="operation">Nombre de la operación</param>
        /// <param name="elapsedMilliseconds">Tiempo transcurrido en milisegundos</param>
        /// <param name="additionalInfo">Información adicional (opcional)</param>
        void LogPerformance(string operation, long elapsedMilliseconds, object additionalInfo = null);

        /// <summary>
        /// Registra un evento de auditoría
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="action">Acción realizada</param>
        /// <param name="resource">Recurso afectado</param>
        /// <param name="result">Resultado de la acción</param>
        /// <param name="additionalInfo">Información adicional (opcional)</param>
        void LogAudit(string userId, string action, string resource, string result, object additionalInfo = null);
    }

    /// <summary>
    /// Niveles de registro
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Information,
        Warning,
        Error,
        Critical
    }
}
