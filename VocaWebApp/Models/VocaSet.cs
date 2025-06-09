using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VocaWebApp.Models
{
    public class VocaSet
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int? FolderId { get; set; }
        public Folder Folder { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Keywords { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }  // public-view, public-copy, private

        public DateTime? LastAccessed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<VocaItem> VocaItems { get; set; }
        public ICollection<ReviewReminder> ReviewReminders { get; set; }
        public ICollection<VocaSetCopy> Copies { get; set; }
    }

}
