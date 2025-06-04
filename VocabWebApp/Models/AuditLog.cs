using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VocabWebApp.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required]
        [MaxLength(255)]
        public string Action { get; set; }

        public string? Details { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
