using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Domain.Models.Auth
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public bool IsLdapUser { get; set; } = false;
    }
}
