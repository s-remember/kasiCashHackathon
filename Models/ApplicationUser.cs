using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace KasiCash.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string BusinessName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string BusinessType { get; set; } = string.Empty;
    }
}