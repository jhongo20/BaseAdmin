using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Domain.Entities;
using AuthSystem.Domain.Interfaces.Repositories;
using AuthSystem.Domain.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace AuthSystem.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserSessionRepository _userSessionRepository;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, IUserSessionRepository userSessionRepository, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _userSessionRepository = userSessionRepository;
            _logger = logger;
        }

        public async Task<string> GenerateTokenAsync(
            Guid userId,
            string username,
            string email,
            IEnumerable<string> roles,
            IEnumerable<string> permissions,
            Guid? organizationId = null,
            IEnumerable<Guid> branchIds = null,
            IDictionary<string, string> additionalClaims = null,
            int expirationMinutes = 60,
            CancellationToken cancellationToken = default)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Agregar roles
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // Agregar permisos
            if (permissions != null)
            {
                foreach (var permission in permissions)
                {
                    claims.Add(new Claim("permission", permission));
                }
            }

            // Agregar organización
            if (organizationId.HasValue)
            {
                claims.Add(new Claim("organizationId", organizationId.Value.ToString()));
            }

            // Agregar sucursales
            if (branchIds != null)
            {
                foreach (var branchId in branchIds)
                {
                    claims.Add(new Claim("branchId", branchId.ToString()));
                }
            }

            // Agregar reclamaciones adicionales
            if (additionalClaims != null)
            {
                foreach (var claim in additionalClaims)
                {
                    claims.Add(new Claim(claim.Key, claim.Value));
                }
            }

            var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshToken = Convert.ToBase64String(randomNumber);

            // Guardar el token de actualización en la base de datos
            var refreshTokenDays = int.Parse(_configuration["JwtSettings:RefreshTokenDurationInDays"]);
            var userSession = new UserSession
            {
                UserId = userId,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenDays),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                UserAgent = "Web", // Información del dispositivo/navegador
                IpAddress = "0.0.0.0" // En un escenario real, esto vendría del contexto HTTP
            };

            await _userSessionRepository.AddAsync(userSession, cancellationToken);
            
            return refreshToken;
        }

        public async Task<bool> ValidateTokenAsync(
            string token,
            bool validateLifetime = true,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])),
                    ValidateLifetime = validateLifetime,
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }

                // Verificar si el token está revocado
                if (await IsTokenRevokedAsync(token, cancellationToken))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["JwtSettings:Issuer"],
                    ValidAudience = _configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"])),
                    ValidateLifetime = false // No validamos la vida útil aquí
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Guid> GetUserIdFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            var principal = await GetPrincipalFromTokenAsync(token, cancellationToken);
            if (principal == null)
                return Guid.Empty;

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Guid.Empty;

            return userId;
        }

        public async Task<string> GetUsernameFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            var principal = await GetPrincipalFromTokenAsync(token, cancellationToken);
            if (principal == null)
                return null;

            return principal.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
        }

        public async Task<IEnumerable<string>> GetRolesFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            var principal = await GetPrincipalFromTokenAsync(token, cancellationToken);
            if (principal == null)
                return Enumerable.Empty<string>();

            var rolesClaims = principal.FindAll(ClaimTypes.Role);
            return rolesClaims.Select(c => c.Value);
        }

        public async Task<IEnumerable<string>> GetPermissionsFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            var principal = await GetPrincipalFromTokenAsync(token, cancellationToken);
            if (principal == null)
                return Enumerable.Empty<string>();

            var permissionsClaims = principal.FindAll("permission");
            return permissionsClaims.Select(c => c.Value);
        }

        public async Task<Guid?> GetOrganizationIdFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            var principal = await GetPrincipalFromTokenAsync(token, cancellationToken);
            if (principal == null)
                return null;

            var organizationIdClaim = principal.FindFirst("organizationId")?.Value;
            if (string.IsNullOrEmpty(organizationIdClaim) || !Guid.TryParse(organizationIdClaim, out var organizationId))
                return null;

            return organizationId;
        }

        public async Task<IEnumerable<Guid>> GetBranchIdsFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            var principal = await GetPrincipalFromTokenAsync(token, cancellationToken);
            if (principal == null)
                return Enumerable.Empty<Guid>();

            var branchIdsClaims = principal.FindAll("branchId");
            var branchIds = new List<Guid>();

            foreach (var claim in branchIdsClaims)
            {
                if (Guid.TryParse(claim.Value, out var branchId))
                {
                    branchIds.Add(branchId);
                }
            }

            return branchIds;
        }

        public async Task<(bool isValid, Guid userId)> ValidateRefreshTokenAsync(
            string refreshToken,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return (false, Guid.Empty);

            try
            {
                // Buscar la sesión por el token de actualización
                var userSession = await _userSessionRepository.GetByRefreshTokenAsync(refreshToken, cancellationToken);
                
                if (userSession == null)
                    return (false, Guid.Empty);
                
                // Verificar si la sesión está activa y no ha expirado
                if (!userSession.IsActive || userSession.IsRevoked || userSession.ExpiresAt <= DateTime.UtcNow)
                    return (false, Guid.Empty);
                
                return (true, userSession.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating refresh token");
                return (false, Guid.Empty);
            }
        }

        public async Task<bool> IsTokenRevokedAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
                return true;

            try
            {
                var jti = await GetJtiFromTokenAsync(token, cancellationToken);
                if (string.IsNullOrEmpty(jti))
                    return true;

                // Aquí se debería verificar si el token está en una lista de revocados
                // Por ahora, simplemente verificamos si la sesión está activa
                var userSession = await _userSessionRepository.GetByTokenIdAsync(jti, cancellationToken);
                return userSession == null || !userSession.IsActive;
            }
            catch
            {
                return true;
            }
        }

        private async Task<string> GetJtiFromTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            var principal = await GetPrincipalFromTokenAsync(token, cancellationToken);
            if (principal == null)
                return null;

            return principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
        }

        public async Task<bool> RevokeTokenAsync(
            string token,
            string reason,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var jti = await GetJtiFromTokenAsync(token, cancellationToken);
                if (string.IsNullOrEmpty(jti))
                    return false;

                var userSession = await _userSessionRepository.GetByTokenIdAsync(jti, cancellationToken);
                if (userSession == null)
                    return false;

                userSession.IsActive = false;
                userSession.LastModifiedAt = DateTime.UtcNow;
                userSession.RevocationReason = reason;
                await _userSessionRepository.UpdateAsync(userSession, cancellationToken);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GeneratePurposeTokenAsync(
            Guid userId,
            string purpose,
            int expirationMinutes,
            IDictionary<string, string> additionalData = null,
            CancellationToken cancellationToken = default)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim("purpose", purpose),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Agregar datos adicionales como claims
            if (additionalData != null)
            {
                foreach (var item in additionalData)
                {
                    claims.Add(new Claim(item.Key, item.Value));
                }
            }

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> ValidatePurposeTokenAsync(
            string token,
            string purpose,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            try
            {
                var principal = await GetPrincipalFromTokenAsync(token, cancellationToken);
                if (principal == null)
                    return false;

                // Verificar el propósito
                var purposeClaim = principal.FindFirst("purpose")?.Value;
                if (string.IsNullOrEmpty(purposeClaim) || purposeClaim != purpose)
                    return false;

                // Verificar el ID de usuario
                var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var tokenUserId) || tokenUserId != userId)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GeneratePasswordResetTokenAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await GeneratePurposeTokenAsync(
                userId,
                "PasswordReset",
                1440, // 24 horas
                null,
                cancellationToken);
        }

        public async Task<Guid?> ValidatePasswordResetTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var principal = await GetPrincipalFromTokenAsync(token, cancellationToken);
                if (principal == null)
                    return null;

                // Verificar el propósito
                var purposeClaim = principal.FindFirst("purpose")?.Value;
                if (string.IsNullOrEmpty(purposeClaim) || purposeClaim != "PasswordReset")
                    return null;

                // Obtener el ID de usuario
                var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return null;

                return userId;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> GenerateAccountActivationTokenAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await GeneratePurposeTokenAsync(
                userId,
                "AccountActivation",
                1440, // 24 horas
                null,
                cancellationToken);
        }

        public async Task<Guid?> ValidateAccountActivationTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            try
            {
                var principal = await GetPrincipalFromTokenAsync(token, cancellationToken);
                if (principal == null)
                    return null;

                // Verificar el propósito
                var purposeClaim = principal.FindFirst("purpose")?.Value;
                if (string.IsNullOrEmpty(purposeClaim) || purposeClaim != "AccountActivation")
                    return null;

                // Obtener el ID de usuario
                var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                    return null;

                return userId;
            }
            catch
            {
                return null;
            }
        }
    }
}
