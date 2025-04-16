using System;
using System.Collections.Generic;
using AuthSystem.Domain.Common.Enums;

namespace AuthSystem.Domain.Models.Users
{
    /// <summary>
    /// Modelo de respuesta para usuarios por módulo
    /// </summary>
    public class UserModuleResponse
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Correo electrónico del usuario
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Estado del usuario
        /// </summary>
        public UserStatus Status { get; set; }

        /// <summary>
        /// Tipo de usuario
        /// </summary>
        public UserType UserType { get; set; }

        /// <summary>
        /// Último inicio de sesión
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// Roles del usuario
        /// </summary>
        public List<UserRoleInfo> Roles { get; set; } = new List<UserRoleInfo>();

        /// <summary>
        /// Información adicional del usuario
        /// </summary>
        public UserDetailInfo DetailInfo { get; set; }
    }

    /// <summary>
    /// Información de rol para un usuario
    /// </summary>
    public class UserRoleInfo
    {
        /// <summary>
        /// ID del rol
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del rol
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del rol
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Indica si el rol está activo
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Fecha de inicio de validez
        /// </summary>
        public DateTime? ValidFrom { get; set; }

        /// <summary>
        /// Fecha de fin de validez
        /// </summary>
        public DateTime? ValidTo { get; set; }
    }

    /// <summary>
    /// Información detallada del usuario
    /// </summary>
    public class UserDetailInfo
    {
        /// <summary>
        /// Número de teléfono
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Última modificación
        /// </summary>
        public DateTime? LastModifiedAt { get; set; }

        /// <summary>
        /// Número de intentos fallidos de inicio de sesión
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// Indica si la cuenta está bloqueada
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// Fecha de desbloqueo (si está bloqueada)
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// Indica si tiene habilitada la autenticación de dos factores
        /// </summary>
        public bool TwoFactorEnabled { get; set; }
    }
}
