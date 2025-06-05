using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VocabWebApp.Models;

namespace VocabWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet cho các bảng tùy chỉnh
        public DbSet<Folder> Folders { get; set; }
        public DbSet<VocabSet> VocaSets { get; set; }
        public DbSet<VocabItem> VocaItems { get; set; }
        public DbSet<VocabItemHistory> VocaItemHistories { get; set; }
        public DbSet<ReviewReminder> ReviewReminders { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<VocabSetCopy> VocaSetCopies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== CẤU HÌNH TRẠNG THÁI VÀ DEFAULT ==========
            modelBuilder.Entity<VocabSet>()
                .Property(v => v.Status)
                .HasMaxLength(20);

            modelBuilder.Entity<VocabItem>()
                .Property(v => v.Status)
                .HasDefaultValue("not_learned")
                .HasMaxLength(20);

            modelBuilder.Entity<VocabItemHistory>()
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
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VocabSet>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VocabSet>()
                .HasOne(v => v.Folder)
                .WithMany(f => f.VocaSets)
                .HasForeignKey(v => v.FolderId)
                .OnDelete(DeleteBehavior.SetNull); // nếu Folder bị xoá, VocaSet giữ nguyên

            modelBuilder.Entity<VocabItem>()
                .HasOne(v => v.VocaSet)
                .WithMany()
                .HasForeignKey(v => v.VocaSetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VocabItemHistory>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VocabItemHistory>()
                .HasOne(h => h.VocaItem)
                .WithMany()
                .HasForeignKey(h => h.VocaItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReviewReminder>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ReviewReminder>()
                .HasOne(r => r.VocaSet)
                .WithMany()
                .HasForeignKey(r => r.VocaSetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AuditLog>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<VocabSetCopy>()
                .HasOne(v => v.OriginalSet)
                .WithMany()
                .HasForeignKey(v => v.OriginalSetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VocabSetCopy>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(v => v.CopiedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== INDEXES ==========
            modelBuilder.Entity<VocabSet>()
                .HasIndex(v => v.UserId)
                .HasDatabaseName("IDX_VocaSet_UserId");

            modelBuilder.Entity<VocabItem>()
                .HasIndex(i => i.VocaSetId)
                .HasDatabaseName("IDX_VocaItem_VocaSetId");

            modelBuilder.Entity<ReviewReminder>()
                .HasIndex(r => r.ReviewDate)
                .HasDatabaseName("IDX_Reminder_ReviewDate");
        }
    }
}
