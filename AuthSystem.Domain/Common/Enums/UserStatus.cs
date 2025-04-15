namespace AuthSystem.Domain.Common.Enums
{
    /// <summary>
    /// Estados posibles de un usuario en el sistema
    /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// Usuario registrado pero pendiente de activación
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Usuario activo y con acceso al sistema
        /// </summary>
        Active = 2,

        /// <summary>
        /// Usuario bloqueado temporalmente (ej. por intentos fallidos de login)
        /// </summary>
        Locked = 3,

        /// <summary>
        /// Usuario inactivo o deshabilitado
        /// </summary>
        Inactive = 4,

        /// <summary>
        /// Usuario eliminado lógicamente
        /// </summary>
        Deleted = 5
    }
}
