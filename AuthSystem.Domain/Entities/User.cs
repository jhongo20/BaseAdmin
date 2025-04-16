using System;
using System.Collections.Generic;
using AuthSystem.Domain.Common.Enums;

namespace AuthSystem.Domain.Entities
{
    /// <summary>
    /// Entidad que representa un usuario en el sistema
    /// </summary>
    public class User : BaseEntity
    {
        /// <summary>
        /// Nombre de usuario único en el sistema
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Hash de la contraseña (solo para usuarios externos)
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Tipo de usuario (interno o externo)
        /// </summary>
        public UserType UserType { get; set; }

        /// <summary>
        /// Estado actual del usuario
        /// </summary>
        public UserStatus Status { get; set; }

        /// <summary>
        /// Número de teléfono del usuario
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Indica si el teléfono ha sido confirmado
        /// </summary>
        public bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Indica si el correo electrónico ha sido confirmado
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Número de intentos fallidos de inicio de sesión consecutivos
        /// </summary>
        public int AccessFailedCount { get; set; }

        /// <summary>
        /// Fecha hasta la que el usuario está bloqueado
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// Indica si el usuario puede ser bloqueado por intentos fallidos
        /// </summary>
        public bool LockoutEnabled { get; set; } = true;

        /// <summary>
        /// Token de seguridad para operaciones como cambio de contraseña
        /// </summary>
        public string SecurityStamp { get; set; }

        /// <summary>
        /// Fecha de último inicio de sesión
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// Fecha de último cambio de contraseña
        /// </summary>
        public DateTime? LastPasswordChangeAt { get; set; }

        /// <summary>
        /// Indica si se debe forzar el cambio de contraseña en el próximo inicio de sesión
        /// </summary>
        public bool ForcePasswordChange { get; set; }

        /// <summary>
        /// Identificador en el directorio activo (solo para usuarios internos)
        /// </summary>
        public string LdapDN { get; set; }

        /// <summary>
        /// Token para restablecimiento de contraseña
        /// </summary>
        public string PasswordResetToken { get; set; }

        /// <summary>
        /// Fecha de expiración del token de restablecimiento de contraseña
        /// </summary>
        public DateTime? PasswordResetTokenExpiry { get; set; }

        /// <summary>
        /// Token para activación de cuenta
        /// </summary>
        public string ActivationToken { get; set; }

        /// <summary>
        /// Fecha de expiración del token de activación
        /// </summary>
        public DateTime? ActivationTokenExpiry { get; set; }

        /// <summary>
        /// Fecha en que se activó la cuenta
        /// </summary>
        public DateTime? ActivatedAt { get; set; }

        /// <summary>
        /// Indica si la autenticación de dos factores está habilitada para el usuario
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// Clave secreta para la autenticación de dos factores
        /// </summary>
        public string TwoFactorSecretKey { get; set; }

        /// <summary>
        /// Token de recuperación para la autenticación de dos factores
        /// </summary>
        public string TwoFactorRecoveryCode { get; set; }

        /// <summary>
        /// Relación con las sesiones activas del usuario
        /// </summary>
        public virtual ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();

        /// <summary>
        /// Relación con los roles asignados al usuario
        /// </summary>
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        /// <summary>
        /// Relación con las sucursales asignadas al usuario
        /// </summary>
        public virtual ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();

        /// <summary>
        /// Verifica si el usuario está bloqueado
        /// </summary>
        /// <returns>True si el usuario está bloqueado</returns>
        public bool IsLockedOut()
        {
            return LockoutEnabled && LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;
        }

        /// <summary>
        /// Incrementa el contador de intentos fallidos y bloquea la cuenta si es necesario
        /// </summary>
        /// <param name="maxFailedAttempts">Número máximo de intentos fallidos permitidos</param>
        /// <param name="lockoutDuration">Duración del bloqueo en minutos</param>
        /// <returns>True si la cuenta ha sido bloqueada</returns>
        public bool IncrementAccessFailedCount(int maxFailedAttempts, int lockoutDuration)
        {
            if (!LockoutEnabled)
                return false;

            AccessFailedCount++;

            if (AccessFailedCount >= maxFailedAttempts)
            {
                LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutDuration);
                Status = UserStatus.Locked;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Resetea el contador de intentos fallidos
        /// </summary>
        public void ResetAccessFailedCount()
        {
            AccessFailedCount = 0;
        }

        /// <summary>
        /// Actualiza la información de inicio de sesión exitoso
        /// </summary>
        public void UpdateOnSuccessfulLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            ResetAccessFailedCount();
            SecurityStamp = Guid.NewGuid().ToString();
        }
    }
}
