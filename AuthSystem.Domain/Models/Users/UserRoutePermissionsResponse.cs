using System;
using System.Collections.Generic;
using AuthSystem.Domain.Common.Enums;

namespace AuthSystem.Domain.Models.Users
{
    /// <summary>
    /// Modelo de respuesta para permisos de usuario en una ruta específica
    /// </summary>
    public class UserRoutePermissionsResponse
    {
        /// <summary>
        /// ID del usuario
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Nombre de usuario
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Nombre completo del usuario
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Ruta solicitada
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Módulo al que pertenece la ruta
        /// </summary>
        public ModuleInfo Module { get; set; }

        /// <summary>
        /// Lista de permisos del usuario para esta ruta
        /// </summary>
        public List<RoutePermissionInfo> Permissions { get; set; } = new List<RoutePermissionInfo>();

        /// <summary>
        /// Indica si el usuario tiene acceso a la ruta
        /// </summary>
        public bool HasAccess { get; set; }
    }

    /// <summary>
    /// Información del módulo
    /// </summary>
    public class ModuleInfo
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
        /// Ruta base del módulo
        /// </summary>
        public string BaseRoute { get; set; }
    }

    /// <summary>
    /// Información de permiso para una ruta
    /// </summary>
    public class RoutePermissionInfo
    {
        /// <summary>
        /// ID del permiso
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nombre del permiso
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descripción del permiso
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Tipo de permiso
        /// </summary>
        public PermissionType Type { get; set; }

        /// <summary>
        /// Rol a través del cual se obtiene este permiso
        /// </summary>
        public string SourceRole { get; set; }

        /// <summary>
        /// ID del rol fuente
        /// </summary>
        public Guid SourceRoleId { get; set; }
    }
}
