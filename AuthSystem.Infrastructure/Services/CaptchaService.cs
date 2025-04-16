using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AuthSystem.Domain.Interfaces;
using AuthSystem.Domain.Interfaces.Services;
using AuthSystem.Domain.Models.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de CAPTCHA
    /// </summary>
    public class CaptchaService : ICaptchaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CaptchaService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly bool _enableCaptcha;
        private readonly int _captchaThreshold;
        
        // Almacén en memoria para los CAPTCHAs generados (en producción debería usar Redis o similar)
        private static readonly ConcurrentDictionary<string, CaptchaInfo> _captchaStore = new ConcurrentDictionary<string, CaptchaInfo>();

        public CaptchaService(
            IUnitOfWork unitOfWork,
            ILogger<CaptchaService> logger,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _configuration = configuration;
            _environment = environment;
            
            // Cargar configuración
            _enableCaptcha = _configuration.GetValue<bool>("AccountLockout:EnableCaptcha", true);
            _captchaThreshold = _configuration.GetValue<int>("AccountLockout:CaptchaThreshold", 2);
        }

        /// <inheritdoc />
        public async Task<CaptchaResponse> GenerateCaptchaAsync()
        {
            if (!_enableCaptcha)
            {
                return new CaptchaResponse { CaptchaRequired = false };
            }

            try
            {
                // Generar un código aleatorio de 6 caracteres
                string captchaCode = GenerateRandomCode(6);
                string captchaId = Guid.NewGuid().ToString();

                // Crear la imagen del CAPTCHA
                string imagePath = await GenerateCaptchaImageAsync(captchaCode, captchaId);
                string imageUrl = $"/captcha/{captchaId}.png";

                // Almacenar el CAPTCHA (con expiración de 5 minutos)
                _captchaStore[captchaId] = new CaptchaInfo
                {
                    Code = captchaCode,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5)
                };

                // Programar la limpieza del CAPTCHA expirado
                CleanupExpiredCaptchasAsync();

                return new CaptchaResponse
                {
                    CaptchaRequired = true,
                    CaptchaId = captchaId,
                    CaptchaImageUrl = imageUrl,
                    Message = "Por favor, ingrese los caracteres que ve en la imagen."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar CAPTCHA");
                return new CaptchaResponse
                {
                    CaptchaRequired = false,
                    Message = "No se pudo generar el CAPTCHA. Intente nuevamente más tarde."
                };
            }
        }

        /// <inheritdoc />
        public async Task<bool> ValidateCaptchaAsync(string captchaId, string userResponse)
        {
            if (!_enableCaptcha || string.IsNullOrEmpty(captchaId) || string.IsNullOrEmpty(userResponse))
            {
                return true;
            }

            if (_captchaStore.TryGetValue(captchaId, out CaptchaInfo captchaInfo))
            {
                if (DateTime.UtcNow > captchaInfo.ExpiresAt)
                {
                    _captchaStore.TryRemove(captchaId, out _);
                    return false;
                }

                bool isValid = string.Equals(captchaInfo.Code, userResponse, StringComparison.OrdinalIgnoreCase);
                
                // Eliminar el CAPTCHA después de la validación
                _captchaStore.TryRemove(captchaId, out _);
                
                // Eliminar la imagen del CAPTCHA
                await Task.Run(() => DeleteCaptchaImageAsync(captchaId));

                return isValid;
            }

            return false;
        }

        /// <inheritdoc />
        public async Task<CaptchaResponse> CheckCaptchaRequirementAsync(string username)
        {
            if (!_enableCaptcha || string.IsNullOrEmpty(username))
            {
                return new CaptchaResponse { CaptchaRequired = false };
            }

            try
            {
                var user = await _unitOfWork.Users.GetByUsernameAsync(username);
                
                // Si el usuario no existe, no mostramos CAPTCHA para evitar enumeración de usuarios
                if (user == null)
                {
                    return new CaptchaResponse { CaptchaRequired = false };
                }

                // Si el usuario tiene más intentos fallidos que el umbral, mostramos CAPTCHA
                if (user.AccessFailedCount >= _captchaThreshold)
                {
                    return await GenerateCaptchaAsync();
                }

                return new CaptchaResponse { CaptchaRequired = false };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar requisito de CAPTCHA para el usuario {Username}", username);
                return new CaptchaResponse { CaptchaRequired = false };
            }
        }

        #region Métodos privados

        /// <summary>
        /// Genera un código aleatorio para el CAPTCHA
        /// </summary>
        /// <param name="length">Longitud del código</param>
        /// <returns>Código aleatorio</returns>
        private string GenerateRandomCode(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

        /// <summary>
        /// Genera una imagen para el CAPTCHA
        /// </summary>
        /// <param name="code">Código del CAPTCHA</param>
        /// <param name="captchaId">Identificador del CAPTCHA</param>
        /// <returns>Ruta de la imagen generada</returns>
        private async Task<string> GenerateCaptchaImageAsync(string code, string captchaId)
        {
            int width = 180;
            int height = 60;
            
            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // Configurar gráficos
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.Clear(Color.White);

                // Dibujar líneas aleatorias
                using (var pen = new Pen(Color.LightGray, 1))
                {
                    for (int i = 0; i < 20; i++)
                    {
                        int x1 = RandomNumberGenerator.GetInt32(0, width);
                        int y1 = RandomNumberGenerator.GetInt32(0, height);
                        int x2 = RandomNumberGenerator.GetInt32(0, width);
                        int y2 = RandomNumberGenerator.GetInt32(0, height);
                        graphics.DrawLine(pen, x1, y1, x2, y2);
                    }
                }

                // Dibujar texto
                using (var font = new Font("Arial", 24, FontStyle.Bold))
                {
                    // Dibujar cada carácter con ligera rotación
                    for (int i = 0; i < code.Length; i++)
                    {
                        using (var brush = new SolidBrush(GetRandomColor()))
                        {
                            // Calcular posición
                            float x = 10 + (i * 25);
                            float y = RandomNumberGenerator.GetInt32(5, 15);

                            // Aplicar transformación para rotación
                            graphics.TranslateTransform(x, y);
                            graphics.RotateTransform(RandomNumberGenerator.GetInt32(-15, 15));
                            graphics.DrawString(code[i].ToString(), font, brush, 0, 0);
                            graphics.ResetTransform();
                        }
                    }
                }

                // Dibujar puntos aleatorios
                for (int i = 0; i < 100; i++)
                {
                    int x = RandomNumberGenerator.GetInt32(0, width);
                    int y = RandomNumberGenerator.GetInt32(0, height);
                    bitmap.SetPixel(x, y, GetRandomColor());
                }

                // Guardar la imagen
                string captchaDirectory = Path.Combine(_environment.WebRootPath, "captcha");
                
                if (!Directory.Exists(captchaDirectory))
                {
                    Directory.CreateDirectory(captchaDirectory);
                }

                string imagePath = Path.Combine(captchaDirectory, $"{captchaId}.png");
                
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    bitmap.Save(stream, ImageFormat.Png);
                }

                return imagePath;
            }
        }

        /// <summary>
        /// Elimina una imagen de CAPTCHA
        /// </summary>
        /// <param name="captchaId">Identificador del CAPTCHA</param>
        private void DeleteCaptchaImageAsync(string captchaId)
        {
            try
            {
                string imagePath = Path.Combine(_environment.WebRootPath, "captcha", $"{captchaId}.png");
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar imagen de CAPTCHA {CaptchaId}", captchaId);
            }
        }

        /// <summary>
        /// Genera un color aleatorio
        /// </summary>
        /// <returns>Color aleatorio</returns>
        private Color GetRandomColor()
        {
            return Color.FromArgb(
                RandomNumberGenerator.GetInt32(0, 100),
                RandomNumberGenerator.GetInt32(0, 100),
                RandomNumberGenerator.GetInt32(0, 100));
        }

        /// <summary>
        /// Limpia los CAPTCHAs expirados
        /// </summary>
        private async void CleanupExpiredCaptchasAsync()
        {
            await Task.Run(() =>
            {
                foreach (var kvp in _captchaStore)
                {
                    if (DateTime.UtcNow > kvp.Value.ExpiresAt)
                    {
                        _captchaStore.TryRemove(kvp.Key, out _);
                        DeleteCaptchaImageAsync(kvp.Key);
                    }
                }
            });
        }

        #endregion

        /// <summary>
        /// Clase interna para almacenar información del CAPTCHA
        /// </summary>
        private class CaptchaInfo
        {
            public string Code { get; set; }
            public DateTime ExpiresAt { get; set; }
        }
    }
}
