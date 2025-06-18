using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VocaWebApp.Models;

namespace VocaWebApp.Data
{
    /// <summary>
    /// Database Context cho ứng dụng học từ vựng
    /// Kế thừa từ IdentityDbContext để tích hợp ASP.NET Core Identity
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet cho các entity chính
        /// <summary>
        /// Bảng lưu trữ thông tin các thư mục
        /// </summary>
        public DbSet<Folder> Folders { get; set; }

        /// <summary>
        /// Bảng lưu trữ thông tin các bộ từ vựng
        /// </summary>
        public DbSet<VocaSet> VocaSets { get; set; }

        /// <summary>
        /// Bảng lưu trữ thông tin các từ vựng
        /// </summary>
        public DbSet<VocaItem> VocaItems { get; set; }

        /// <summary>
        /// Bảng lưu trữ lịch sử học tập từ vựng
        /// </summary>
        public DbSet<VocaItemHistory> VocaItemHistories { get; set; }

        /// <summary>
        /// Bảng lưu trữ các lịch nhắc ôn tập
        /// </summary>
        public DbSet<ReviewReminder> ReviewReminders { get; set; }

        /// <summary>
        /// Bảng lưu trữ lịch sử copy bộ từ vựng
        /// </summary>
        public DbSet<VocaSetCopy> VocaSetCopies { get; set; }

        /// <summary>
        /// Bảng lưu trữ log hoạt động của hệ thống
        /// </summary>
        public DbSet<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// Bảng lưu trữ cài đặt cá nhân của user
        /// </summary>
        public DbSet<UserSettings> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Cấu hình quan hệ và ràng buộc cho các entity

            #region ApplicationUser Configuration
            // Cấu hình quan hệ một-một giữa ApplicationUser và UserSettings
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.UserSettings)
                .WithOne(s => s.User)
                .HasForeignKey<UserSettings>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index cho ApplicationUser để tối ưu performance
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.IsActive)
                .HasDatabaseName("IX_AspNetUsers_IsActive");

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.CreatedAt)
                .HasDatabaseName("IX_AspNetUsers_CreatedAt");

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.LastLoginAt)
                .HasDatabaseName("IX_AspNetUsers_LastLoginAt");
            #endregion

            #region Folder Configuration
            // Cấu hình quan hệ giữa Folder và ApplicationUser
            builder.Entity<Folder>()
                .HasOne(f => f.User)
                .WithMany(u => u.Folders)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index cho Folder để tối ưu tìm kiếm
            builder.Entity<Folder>()
                .HasIndex(f => f.UserId)
                .HasDatabaseName("IX_Folders_UserId");

            builder.Entity<Folder>()
                .HasIndex(f => f.CreatedAt)
                .HasDatabaseName("IX_Folders_CreatedAt");
            #endregion

            #region VocaSet Configuration
            // Cấu hình quan hệ giữa VocaSet và ApplicationUser
            builder.Entity<VocaSet>()
                .HasOne(v => v.User)
                .WithMany(u => u.VocaSets)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ giữa VocaSet và Folder (nullable)
            builder.Entity<VocaSet>()
                .HasOne(v => v.Folder)
                .WithMany(f => f.VocaSets)
                .HasForeignKey(v => v.FolderId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // Indexes quan trọng cho VocaSet để tối ưu performance
            builder.Entity<VocaSet>()
                .HasIndex(v => new { v.Status, v.IsDeleted })
                .HasDatabaseName("IX_VocaSets_Status_IsDeleted");

            builder.Entity<VocaSet>()
                .HasIndex(v => new { v.UserId, v.LastAccessed })
                .HasDatabaseName("IX_VocaSets_UserId_LastAccessed");

            builder.Entity<VocaSet>()
                .HasIndex(v => v.ViewCount)
                .HasDatabaseName("IX_VocaSets_ViewCount");

            builder.Entity<VocaSet>()
                .HasIndex(v => v.CreatedAt)
                .HasDatabaseName("IX_VocaSets_CreatedAt");

            builder.Entity<VocaSet>()
                .HasIndex(v => v.IsHidden)
                .HasDatabaseName("IX_VocaSets_IsHidden");

            builder.Entity<VocaSet>()
                .HasIndex(v => v.Keywords)
                .HasDatabaseName("IX_VocaSets_Keywords");
            #endregion

            #region VocaItem Configuration
            // Cấu hình quan hệ giữa VocaItem và VocaSet
            builder.Entity<VocaItem>()
                .HasOne(v => v.VocaSet)
                .WithMany(s => s.VocaItems)
                .HasForeignKey(v => v.VocaSetId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index cho VocaItem
            builder.Entity<VocaItem>()
                .HasIndex(v => v.VocaSetId)
                .HasDatabaseName("IX_VocaItems_VocaSetId");

            builder.Entity<VocaItem>()
                .HasIndex(v => v.Status)
                .HasDatabaseName("IX_VocaItems_Status");

            builder.Entity<VocaItem>()
                .HasIndex(v => v.Word)
                .HasDatabaseName("IX_VocaItems_Word");
            #endregion

            #region VocaItemHistory Configuration
            // Cấu hình quan hệ giữa VocaItemHistory và ApplicationUser
            builder.Entity<VocaItemHistory>()
                .HasOne(h => h.User)
                .WithMany(u => u.VocaItemHistories)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ giữa VocaItemHistory và VocaItem
            builder.Entity<VocaItemHistory>()
                .HasOne(h => h.VocaItem)
                .WithMany(v => v.Histories)
                .HasForeignKey(h => h.VocaItemId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // Index cho VocaItemHistory
            builder.Entity<VocaItemHistory>()
                .HasIndex(h => new { h.UserId, h.VocaItemId })
                .HasDatabaseName("IX_VocaItemHistories_UserId_VocaItemId");

            builder.Entity<VocaItemHistory>()
                .HasIndex(h => h.ReviewedAt)
                .HasDatabaseName("IX_VocaItemHistories_ReviewedAt");
            #endregion

            #region ReviewReminder Configuration
            // Cấu hình quan hệ giữa ReviewReminder và ApplicationUser
            builder.Entity<ReviewReminder>()
                .HasOne(r => r.User)
                .WithMany(u => u.ReviewReminders)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ giữa ReviewReminder và VocaSet
            builder.Entity<ReviewReminder>()
                .HasOne(r => r.VocaSet)
                .WithMany(v => v.ReviewReminders)
                .HasForeignKey(r => r.VocaSetId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // Index cho ReviewReminder
            builder.Entity<ReviewReminder>()
                .HasIndex(r => new { r.ReviewDate, r.IsSent })
                .HasDatabaseName("IX_ReviewReminders_ReviewDate_IsSent");

            builder.Entity<ReviewReminder>()
                .HasIndex(r => r.UserId)
                .HasDatabaseName("IX_ReviewReminders_UserId");
            #endregion

            #region VocaSetCopy Configuration
            // Cấu hình quan hệ giữa VocaSetCopy và VocaSet (bộ gốc)
            builder.Entity<VocaSetCopy>()
                .HasOne(c => c.OriginalSet)
                .WithMany(v => v.Copies)
                .HasForeignKey(c => c.OriginalSetId)
                .OnDelete(DeleteBehavior.ClientCascade);

            // Cấu hình quan hệ giữa VocaSetCopy và ApplicationUser (người copy)
            builder.Entity<VocaSetCopy>()
                .HasOne(c => c.CopiedByUser)
                .WithMany(u => u.VocaSetCopies)
                .HasForeignKey(c => c.CopiedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index cho VocaSetCopy
            builder.Entity<VocaSetCopy>()
                .HasIndex(c => c.OriginalSetId)
                .HasDatabaseName("IX_VocaSetCopies_OriginalSetId");

            builder.Entity<VocaSetCopy>()
                .HasIndex(c => c.CopiedByUserId)
                .HasDatabaseName("IX_VocaSetCopies_CopiedByUserId");

            builder.Entity<VocaSetCopy>()
                .HasIndex(c => c.CopiedAt)
                .HasDatabaseName("IX_VocaSetCopies_CopiedAt");
            #endregion

            #region AuditLog Configuration
            // Cấu hình quan hệ giữa AuditLog và ApplicationUser (nullable)
            builder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Index cho AuditLog
            builder.Entity<AuditLog>()
                .HasIndex(a => a.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");

            builder.Entity<AuditLog>()
                .HasIndex(a => a.CreatedAt)
                .HasDatabaseName("IX_AuditLogs_CreatedAt");

            builder.Entity<AuditLog>()
                .HasIndex(a => a.Action)
                .HasDatabaseName("IX_AuditLogs_Action");
            #endregion

            #region UserSettings Configuration
            // Index cho UserSettings
            builder.Entity<UserSettings>()
                .HasIndex(s => s.UserId)
                .IsUnique()
                .HasDatabaseName("IX_UserSettings_UserId");

            builder.Entity<UserSettings>()
                .HasIndex(s => s.PreferredLanguage)
                .HasDatabaseName("IX_UserSettings_PreferredLanguage");
            #endregion

            // Cấu hình các ràng buộc và default values bổ sung
            ConfigureDefaultValues(builder);
        }

        /// <summary>
        /// Cấu hình các giá trị mặc định cho các entity
        /// </summary>
        private void ConfigureDefaultValues(ModelBuilder builder)
        {
            // Cấu hình default values cho ApplicationUser
            builder.Entity<ApplicationUser>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

            builder.Entity<ApplicationUser>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Cấu hình default values cho VocaSet
            builder.Entity<VocaSet>()
                .Property(v => v.Status)
                .HasDefaultValue("private");

            builder.Entity<VocaSet>()
                .Property(v => v.IsDeleted)
                .HasDefaultValue(false);

            builder.Entity<VocaSet>()
                .Property(v => v.IsHidden)
                .HasDefaultValue(false);

            builder.Entity<VocaSet>()
                .Property(v => v.ViewCount)
                .HasDefaultValue(0);

            builder.Entity<VocaSet>()
                .Property(v => v.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Cấu hình default values cho VocaItem
            builder.Entity<VocaItem>()
                .Property(v => v.Status)
                .HasDefaultValue("notlearned");

            // Cấu hình default values cho ReviewReminder
            builder.Entity<ReviewReminder>()
                .Property(r => r.IsEmail)
                .HasDefaultValue(false);

            builder.Entity<ReviewReminder>()
                .Property(r => r.IsNotification)
                .HasDefaultValue(true);

            builder.Entity<ReviewReminder>()
                .Property(r => r.IsSent)
                .HasDefaultValue(false);

            // Cấu hình default values cho UserSettings
            builder.Entity<UserSettings>()
                .Property(s => s.DailyGoal)
                .HasDefaultValue(10);

            builder.Entity<UserSettings>()
                .Property(s => s.EmailNotifications)
                .HasDefaultValue(true);

            builder.Entity<UserSettings>()
                .Property(s => s.WebNotifications)
                .HasDefaultValue(true);

            builder.Entity<UserSettings>()
                .Property(s => s.PreferredLanguage)
                .HasDefaultValue("vi");

            builder.Entity<UserSettings>()
                .Property(s => s.Theme)
                .HasDefaultValue("light");

            builder.Entity<UserSettings>()
                .Property(s => s.AutoPlayAudio)
                .HasDefaultValue(false);

            builder.Entity<UserSettings>()
                .Property(s => s.DefaultReviewInterval)
                .HasDefaultValue(7);

            builder.Entity<UserSettings>()
                .Property(s => s.FlashcardSessionSize)
                .HasDefaultValue(20);

            builder.Entity<UserSettings>()
                .Property(s => s.ShowPronunciation)
                .HasDefaultValue(true);

            builder.Entity<UserSettings>()
                .Property(s => s.ShowWordType)
                .HasDefaultValue(true);

            builder.Entity<UserSettings>()
                .Property(s => s.DefaultFlashcardMode)
                .HasDefaultValue("word-to-meaning");

            builder.Entity<UserSettings>()
                .Property(s => s.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<UserSettings>()
                .Property(s => s.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Cấu hình timestamps cho các entity khác
            builder.Entity<Folder>()
                .Property(f => f.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<VocaItemHistory>()
                .Property(h => h.ReviewedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<VocaSetCopy>()
                .Property(c => c.CopiedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Entity<AuditLog>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}