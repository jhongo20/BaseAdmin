using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;

namespace AuthSystem.Domain.Interfaces.Repositories
{
    /// <summary>
    /// Interfaz para el repositorio de intentos de inicio de sesión
    /// </summary>
    public interface IUserLoginAttemptRepository : IRepository<UserLoginAttempt>
    {
        /// <summary>
        /// Obtiene los intentos de inicio de sesión de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de intentos de inicio de sesión</returns>
        Task<IReadOnlyList<UserLoginAttempt>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los intentos de inicio de sesión fallidos recientes de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="minutes">Minutos hacia atrás para considerar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de intentos de inicio de sesión fallidos</returns>
        Task<IReadOnlyList<UserLoginAttempt>> GetRecentFailedByUserIdAsync(Guid userId, int minutes = 30, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los intentos de inicio de sesión fallidos recientes desde una dirección IP
        /// </summary>
        /// <param name="ipAddress">Dirección IP</param>
        /// <param name="minutes">Minutos hacia atrás para considerar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de intentos de inicio de sesión fallidos</returns>
        Task<IReadOnlyList<UserLoginAttempt>> GetRecentFailedByIpAddressAsync(string ipAddress, int minutes = 30, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cuenta los intentos de inicio de sesión fallidos recientes de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="minutes">Minutos hacia atrás para considerar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de intentos fallidos</returns>
        Task<int> CountRecentFailedByUserIdAsync(Guid userId, int minutes = 30, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cuenta los intentos de inicio de sesión fallidos recientes desde una dirección IP
        /// </summary>
        /// <param name="ipAddress">Dirección IP</param>
        /// <param name="minutes">Minutos hacia atrás para considerar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Número de intentos fallidos</returns>
        Task<int> CountRecentFailedByIpAddressAsync(string ipAddress, int minutes = 30, CancellationToken cancellationToken = default);
    }
}
