using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Domain.Models.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
