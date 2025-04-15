namespace AuthSystem.Domain.Common.Enums
{
    /// <summary>
    /// Tipos de usuario en el sistema
    /// </summary>
    public enum UserType
    {
        /// <summary>
        /// Usuario interno autenticado mediante LDAP/Active Directory
        /// </summary>
        Internal = 1,

        /// <summary>
        /// Usuario externo autenticado mediante credenciales locales
        /// </summary>
        External = 2
    }
}
