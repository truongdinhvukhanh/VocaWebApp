using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VocaWebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        public ICollection<Folder> Folders { get; set; }
        public ICollection<AuditLog> AuditLogs { get; set; }
        public ICollection<ReviewReminder> ReviewReminders { get; set; }
        public ICollection<VocaItemHistory> VocaItemHistories { get; set; }
        public ICollection<VocaSet> VocaSets { get; set; }
        public ICollection<VocaSetCopy> VocaSetCopies { get; set; }
    }
}