using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VocaWebApp.Models;

namespace VocaWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet cho các bảng tùy chỉnh
        public DbSet<Folder> Folders { get; set; }
        public DbSet<VocaSet> VocaSets { get; set; }
        public DbSet<VocaItem> VocaItems { get; set; }
        public DbSet<VocaItemHistory> VocaItemHistories { get; set; }
        public DbSet<ReviewReminder> ReviewReminders { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<VocaSetCopy> VocaSetCopies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== CẤU HÌNH TRẠNG THÁI VÀ DEFAULT ==========
            modelBuilder.Entity<VocaSet>()
                .Property(v => v.Status)
                .HasMaxLength(20);

            modelBuilder.Entity<VocaItem>()
                .Property(v => v.Status)
                .HasDefaultValue("not_learned")
                .HasMaxLength(20);

            modelBuilder.Entity<VocaItemHistory>()
                .Property(h => h.Status)
                .HasMaxLength(20);

            modelBuilder.Entity<ReviewReminder>()
                .Property(r => r.IsEmail)
                .HasDefaultValue(false);

            modelBuilder.Entity<ReviewReminder>()
                .Property(r => r.IsNotification)
                .HasDefaultValue(true);

            modelBuilder.Entity<ReviewReminder>()
                .Property(r => r.IsSent)
                .HasDefaultValue(false);

            modelBuilder.Entity<AuditLog>()
                .Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(255);

            // ========== CẤU HÌNH FOREIGN KEY - TRÁNH CASCADE VÒNG ==========
            modelBuilder.Entity<Folder>()
                .HasOne(f => f.User)
                .WithMany(u => u.Folders)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VocaSet>()
                .HasOne(v => v.User)
                .WithMany(u => u.VocaSets)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VocaSet>()
                .HasOne(v => v.Folder)
                .WithMany(f => f.VocaSets)
                .HasForeignKey(v => v.FolderId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<VocaItem>()
                .HasOne(v => v.VocaSet)
                .WithMany(s => s.VocaItems) 
                .HasForeignKey(v => v.VocaSetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VocaItemHistory>()
                .HasOne(h => h.User)
                .WithMany(u => u.VocaItemHistories)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VocaItemHistory>()
                .HasOne(h => h.VocaItem)
                .WithMany(i => i.Histories)
                .HasForeignKey(h => h.VocaItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReviewReminder>()
                .HasOne(r => r.User)
                .WithMany(u => u.ReviewReminders)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReviewReminder>()
                .HasOne(r => r.VocaSet)
                .WithMany(s => s.ReviewReminders)
                .HasForeignKey(r => r.VocaSetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<VocaSetCopy>()
                .HasOne(c => c.OriginalSet)
                .WithMany(s => s.Copies)
                .HasForeignKey(c => c.OriginalSetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VocaSetCopy>()
                .HasOne(c => c.CopiedByUser)
                .WithMany(u => u.VocaSetCopies)
                .HasForeignKey(c => c.CopiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // ========== INDEXES ==========
            modelBuilder.Entity<VocaSet>()
                .HasIndex(v => v.UserId)
                .HasDatabaseName("IDX_VocaSet_UserId");

            modelBuilder.Entity<VocaItem>()
                .HasIndex(i => i.VocaSetId)
                .HasDatabaseName("IDX_VocaItem_VocaSetId");

            modelBuilder.Entity<ReviewReminder>()
                .HasIndex(r => r.ReviewDate)
                .HasDatabaseName("IDX_Reminder_ReviewDate");
        }
    }
}
