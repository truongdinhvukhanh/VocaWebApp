using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VocabWebApp.Models
{
    public class Folder
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<VocaSet> VocaSets { get; set; }
    }

}
