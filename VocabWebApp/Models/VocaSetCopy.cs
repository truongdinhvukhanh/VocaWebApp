using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VocabWebApp.Models
{
    public class VocaSetCopy
    {
        public int Id { get; set; }

        [Required]
        public int OriginalSetId { get; set; }
        public VocaSet OriginalSet { get; set; }

        [Required]
        public string CopiedByUserId { get; set; }
        public IdentityUser CopiedByUser { get; set; }

        public DateTime CopiedAt { get; set; } = DateTime.UtcNow;
    }

}
