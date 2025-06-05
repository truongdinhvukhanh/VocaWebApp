using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VocabWebApp.Models
{
    public class ReviewReminder
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        public int VocaSetId { get; set; }
        public VocabSet VocaSet { get; set; }

        public DateTime ReviewDate { get; set; }

        public int? RepeatIntervalDays { get; set; }

        public bool IsEmail { get; set; } = false;
        public bool IsNotification { get; set; } = true;
        public bool IsSent { get; set; } = false;
    }

}
