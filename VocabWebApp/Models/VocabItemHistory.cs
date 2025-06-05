using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VocabWebApp.Models
{
    public class VocabItemHistory
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public int VocaItemId { get; set; }
        public VocabItem VocaItem { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }  // learned / not_learned

        public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
    }

}
