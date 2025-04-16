using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Services
{
    /// <summary>
    /// Interfaz para el servicio de bloqueo de cuentas
    /// </summary>
    public interface IAccountLockoutService
    {
        /// <summary>
        /// Verifica si una cuenta está bloqueada
        /// </summary>
        /// <param name="user">Usuario a verificar</param>
        /// <returns>True si la cuenta está bloqueada</returns>
        bool IsAccountLocked(User user);

        /// <summary>
        /// Registra un intento fallido de inicio de sesión y bloquea la cuenta si es necesario
        /// </summary>
        /// <param name="user">Usuario que ha fallado el inicio de sesión</param>
        /// <returns>True si la cuenta ha sido bloqueada</returns>
        Task<bool> RecordFailedLoginAttemptAsync(User user);

        /// <summary>
        /// Registra un inicio de sesión exitoso y resetea el contador de intentos fallidos
        /// </summary>
        /// <param name="user">Usuario que ha iniciado sesión correctamente</param>
        Task RecordSuccessfulLoginAttemptAsync(User user);

        /// <summary>
        /// Desbloquea una cuenta de usuario
        /// </summary>
        /// <param name="user">Usuario a desbloquear</param>
        Task UnlockAccountAsync(User user);
    }
}
