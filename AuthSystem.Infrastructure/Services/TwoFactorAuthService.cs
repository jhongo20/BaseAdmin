using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OtpNet;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de autenticación de dos factores
    /// </summary>
    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TwoFactorAuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _applicationName;
        
        // Almacén en memoria para los tokens de sesión (en producción debería usar Redis o similar)
        private static readonly ConcurrentDictionary<string, SessionTokenInfo> _sessionTokenStore = 
            new ConcurrentDictionary<string, SessionTokenInfo>();

        public TwoFactorAuthService(
            IUnitOfWork unitOfWork,
            ILogger<TwoFactorAuthService> logger,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            _applicationName = _configuration.GetValue<string>("ApplicationName", "AuthSystem");
        }

        /// <inheritdoc />
        public async Task<TwoFactorSetupResponse> GenerateSetupInfoAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                // Generar una clave secreta aleatoria
                var secretKey = GenerateSecretKey();
                
                // Guardar la clave secreta en el usuario (sin activar 2FA aún)
                user.TwoFactorSecretKey = secretKey;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Generar la URL para el código QR
                string qrCodeUrl = GenerateQrCodeUrl(secretKey, user.Username);

                return new TwoFactorSetupResponse
                {
                    SecretKey = secretKey,
                    QrCodeUrl = qrCodeUrl,
                    ApplicationName = _applicationName,
                    Username = user.Username
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar información de configuración de 2FA para el usuario {Username}", user.Username);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> EnableTwoFactorAsync(User user, string verificationCode)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(user.TwoFactorSecretKey))
                return false;

            try
            {
                // Verificar el código proporcionado
                bool isCodeValid = VerifyCode(user.TwoFactorSecretKey, verificationCode);
                if (!isCodeValid)
                    return false;

                // Habilitar 2FA y generar un código de recuperación
                user.TwoFactorEnabled = true;
                user.TwoFactorRecoveryCode = GenerateRecoveryCode();
                
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Autenticación de dos factores habilitada para el usuario {Username}", user.Username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al habilitar 2FA para el usuario {Username}", user.Username);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DisableTwoFactorAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                user.TwoFactorEnabled = false;
                user.TwoFactorSecretKey = null;
                user.TwoFactorRecoveryCode = null;
                
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Autenticación de dos factores deshabilitada para el usuario {Username}", user.Username);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al deshabilitar 2FA para el usuario {Username}", user.Username);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> VerifyCodeAsync(User user, string verificationCode)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecretKey))
                return false;

            try
            {
                // Verificar si es un código de recuperación
                if (!string.IsNullOrEmpty(user.TwoFactorRecoveryCode) && 
                    user.TwoFactorRecoveryCode.Equals(verificationCode, StringComparison.OrdinalIgnoreCase))
                {
                    // Generar un nuevo código de recuperación después de usar el actual
                    user.TwoFactorRecoveryCode = GenerateRecoveryCode();
                    await _unitOfWork.Users.UpdateAsync(user);
                    await _unitOfWork.SaveChangesAsync();
                    
                    _logger.LogWarning("Código de recuperación utilizado por el usuario {Username}", user.Username);
                    return true;
                }

                // Verificar el código TOTP
                return VerifyCode(user.TwoFactorSecretKey, verificationCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar código 2FA para el usuario {Username}", user.Username);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<string> GenerateSessionTokenAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                // Generar un token aleatorio
                string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
                
                // Almacenar el token con una expiración de 5 minutos
                _sessionTokenStore[user.Username] = new SessionTokenInfo
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                };

                // Programar limpieza de tokens expirados
                CleanupExpiredSessionTokensAsync();

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar token de sesión para el usuario {Username}", user.Username);
                throw;
            }
        }

        /// <inheritdoc />
        public Task<bool> ValidateSessionTokenAsync(string username, string sessionToken)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(sessionToken))
                return Task.FromResult(false);

            try
            {
                if (_sessionTokenStore.TryGetValue(username, out SessionTokenInfo tokenInfo))
                {
                    if (DateTime.UtcNow > tokenInfo.ExpiresAt)
                    {
                        _sessionTokenStore.TryRemove(username, out _);
                        return Task.FromResult(false);
                    }

                    bool isValid = string.Equals(tokenInfo.Token, sessionToken);
                    
                    // Eliminar el token después de la validación
                    if (isValid)
                    {
                        _sessionTokenStore.TryRemove(username, out _);
                    }

                    return Task.FromResult(isValid);
                }

                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar token de sesión para el usuario {Username}", username);
                return Task.FromResult(false);
            }
        }

        #region Métodos privados

        /// <summary>
        /// Genera una clave secreta aleatoria para TOTP
        /// </summary>
        /// <returns>Clave secreta en formato Base32</returns>
        private string GenerateSecretKey()
        {
            byte[] secretKey = RandomNumberGenerator.GetBytes(20); // 160 bits
            return Base32Encoding.ToString(secretKey);
        }

        /// <summary>
        /// Genera la URL para el código QR
        /// </summary>
        /// <param name="secretKey">Clave secreta</param>
        /// <param name="username">Nombre de usuario</param>
        /// <returns>URL para el código QR</returns>
        private string GenerateQrCodeUrl(string secretKey, string username)
        {
            string issuer = Uri.EscapeDataString(_applicationName);
            string account = Uri.EscapeDataString(username);
            string secret = Uri.EscapeDataString(secretKey);
            
            return $"otpauth://totp/{issuer}:{account}?secret={secret}&issuer={issuer}";
        }

        /// <summary>
        /// Verifica un código TOTP
        /// </summary>
        /// <param name="secretKey">Clave secreta</param>
        /// <param name="verificationCode">Código de verificación</param>
        /// <returns>True si el código es válido</returns>
        private bool VerifyCode(string secretKey, string verificationCode)
        {
            try
            {
                var secretKeyBytes = Base32Encoding.ToBytes(secretKey);
                var totp = new Totp(secretKeyBytes);
                
                // Verificar el código con una ventana de tiempo de ±30 segundos
                return totp.VerifyTotp(verificationCode, out _, new VerificationWindow(1, 1));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Genera un código de recuperación
        /// </summary>
        /// <returns>Código de recuperación</returns>
        private string GenerateRecoveryCode()
        {
            // Generar un código alfanumérico de 10 caracteres
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            var result = new StringBuilder(10);

            for (int i = 0; i < 10; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Limpia los tokens de sesión expirados
        /// </summary>
        private async void CleanupExpiredSessionTokensAsync()
        {
            await Task.Run(() =>
            {
                foreach (var kvp in _sessionTokenStore)
                {
                    if (DateTime.UtcNow > kvp.Value.ExpiresAt)
                    {
                        _sessionTokenStore.TryRemove(kvp.Key, out _);
                    }
                }
            });
        }

        #endregion

        /// <summary>
        /// Clase interna para almacenar información del token de sesión
        /// </summary>
        private class SessionTokenInfo
        {
            public string Token { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
