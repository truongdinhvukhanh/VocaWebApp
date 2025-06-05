using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VocabWebApp.Models
{
    public class VocabSetCopy
    {
        public int Id { get; set; }

        [Required]
        public int OriginalSetId { get; set; }
        public VocabSet OriginalSet { get; set; }

        [Required]
        public string CopiedByUserId { get; set; }
        public ApplicationUser CopiedByUser { get; set; }

        public DateTime CopiedAt { get; set; } = DateTime.UtcNow;
    }

}
