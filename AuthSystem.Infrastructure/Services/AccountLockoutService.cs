using System;
using System.Threading.Tasks;
using AuthSystem.Domain.Common.Enums;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de bloqueo de cuentas
    /// </summary>
    public class AccountLockoutService : IAccountLockoutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AccountLockoutService> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _maxFailedAttempts;
        private readonly int _lockoutDurationMinutes;
        private readonly bool _enableAccountLockout;

        public AccountLockoutService(
            IUnitOfWork unitOfWork,
            ILogger<AccountLockoutService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            
            // Cargar configuración
            _maxFailedAttempts = _configuration.GetValue<int>("AccountLockout:MaxFailedAttempts", 5);
            _lockoutDurationMinutes = _configuration.GetValue<int>("AccountLockout:LockoutDurationMinutes", 15);
            _enableAccountLockout = _configuration.GetValue<bool>("AccountLockout:EnableAccountLockout", true);
        }

        /// <inheritdoc />
        public bool IsAccountLocked(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!_enableAccountLockout || !user.LockoutEnabled)
                return false;

            return user.IsLockedOut();
        }

        /// <inheritdoc />
        public async Task<bool> RecordFailedLoginAttemptAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!_enableAccountLockout || !user.LockoutEnabled)
                return false;

            bool isLocked = user.IncrementAccessFailedCount(_maxFailedAttempts, _lockoutDurationMinutes);
            
            if (isLocked)
            {
                _logger.LogWarning(
                    "La cuenta del usuario {Username} ha sido bloqueada por {Duration} minutos después de {Attempts} intentos fallidos",
                    user.Username, _lockoutDurationMinutes, _maxFailedAttempts);
            }
            else
            {
                _logger.LogInformation(
                    "Intento de inicio de sesión fallido para el usuario {Username}. Intentos fallidos: {FailedCount}/{MaxAttempts}",
                    user.Username, user.AccessFailedCount, _maxFailedAttempts);
            }

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return isLocked;
        }

        /// <inheritdoc />
        public async Task RecordSuccessfulLoginAttemptAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.UpdateOnSuccessfulLogin();
            
            // Si el usuario estaba bloqueado, lo desbloqueamos
            if (user.Status == UserStatus.Locked)
            {
                user.Status = UserStatus.Active;
                user.LockoutEnd = null;
                _logger.LogInformation("La cuenta del usuario {Username} ha sido desbloqueada después de un inicio de sesión exitoso", user.Username);
            }

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task UnlockAccountAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.AccessFailedCount = 0;
            user.LockoutEnd = null;
            
            if (user.Status == UserStatus.Locked)
            {
                user.Status = UserStatus.Active;
            }

            _logger.LogInformation("La cuenta del usuario {Username} ha sido desbloqueada manualmente", user.Username);

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
