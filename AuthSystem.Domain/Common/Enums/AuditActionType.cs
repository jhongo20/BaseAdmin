namespace AuthSystem.Domain.Common.Enums
{
    /// <summary>
    /// Tipos de acciones que se registran en la auditoría
    /// </summary>
    public enum AuditActionType
    {
        /// <summary>
        /// Creación de un nuevo registro
        /// </summary>
        Create = 1,

        /// <summary>
        /// Actualización de un registro existente
        /// </summary>
        Update = 2,

        /// <summary>
        /// Eliminación de un registro
        /// </summary>
        Delete = 3,

        /// <summary>
        /// Acceso a un recurso o información
        /// </summary>
        Read = 4,

        /// <summary>
        /// Inicio de sesión en el sistema
        /// </summary>
        Login = 5,

        /// <summary>
        /// Intento fallido de inicio de sesión
        /// </summary>
        FailedLogin = 6,

        /// <summary>
        /// Cierre de sesión
        /// </summary>
        Logout = 7,

        /// <summary>
        /// Cambio de contraseña
        /// </summary>
        PasswordChange = 8,

        /// <summary>
        /// Cambio en permisos o roles
        /// </summary>
        PermissionChange = 9,

        /// <summary>
        /// Bloqueo de cuenta de usuario
        /// </summary>
        AccountLock = 10,

        /// <summary>
        /// Desbloqueo de cuenta de usuario
        /// </summary>
        AccountUnlock = 11,

        /// <summary>
        /// Exportación de datos
        /// </summary>
        Export = 12,

        /// <summary>
        /// Importación de datos
        /// </summary>
        Import = 13
    }
}
