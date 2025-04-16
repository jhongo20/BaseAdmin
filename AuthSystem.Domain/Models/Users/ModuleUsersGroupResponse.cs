using System;
using System.Collections.Generic;

namespace AuthSystem.Domain.Models.Users
{
    /// <summary>
    /// Modelo de respuesta para usuarios agrupados por módulo
    /// </summary>
    public class ModuleUsersGroupResponse
    {
        /// <summary>
        /// ID del módulo
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del módulo
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del módulo
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ruta del módulo
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Icono del módulo
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Indica si el módulo está habilitado
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Cantidad de usuarios con acceso al módulo
        /// </summary>
        public int UserCount { get; set; }

        /// <summary>
        /// Lista de usuarios con acceso al módulo
        /// </summary>
        public List<ModuleUserInfo> Users { get; set; } = new List<ModuleUserInfo>();
    }

    /// <summary>
    /// Información resumida de usuario para agrupación por módulo
    /// </summary>
    public class ModuleUserInfo
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
        /// Roles del usuario en este módulo
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Estado del usuario
        /// </summary>
        public string Status { get; set; }
    }
}
