namespace AuthSystem.Domain.Common.Enums
{
    /// <summary>
    /// Tipos de permisos en el sistema
    /// </summary>
    public enum PermissionType
    {
        /// <summary>
        /// Permiso para ver información
        /// </summary>
        Read = 1,

        /// <summary>
        /// Permiso para crear nuevos registros
        /// </summary>
        Create = 2,

        /// <summary>
        /// Permiso para actualizar registros existentes
        /// </summary>
        Update = 3,

        /// <summary>
        /// Permiso para eliminar registros
        /// </summary>
        Delete = 4,

        /// <summary>
        /// Permiso para aprobar o autorizar acciones
        /// </summary>
        Approve = 5,

        /// <summary>
        /// Permiso para administrar completamente un módulo
        /// </summary>
        Admin = 6,

        /// <summary>
        /// Permiso para exportar datos
        /// </summary>
        Export = 7,

        /// <summary>
        /// Permiso para importar datos
        /// </summary>
        Import = 8
    }
}
