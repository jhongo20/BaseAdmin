using System;

namespace AuthSystem.Domain.Models.Auth
{
    public class AuthResponse
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpiration { get; set; }
        public bool IsLdapUser { get; set; }
        public string[] Roles { get; set; }
        public string[] Permissions { get; set; }
    }
}
