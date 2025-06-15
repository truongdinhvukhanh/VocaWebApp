using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VocaWebApp.Models
{
    /// <summary>
    /// Model đại diện cho người dùng trong hệ thống học từ vựng
    /// Kế thừa từ IdentityUser để tích hợp với ASP.NET Core Identity
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Họ và tên đầy đủ của người dùng
        /// Bắt buộc nhập, tối đa 100 ký tự
        /// Dùng để hiển thị trong giao diện và báo cáo
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của tài khoản
        /// true = tài khoản đang hoạt động, false = bị khóa/vô hiệu hóa
        /// Admin sử dụng để quản lý quyền truy cập của người dùng
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Thời gian tạo tài khoản
        /// Tự động gán khi tạo tài khoản mới
        /// Dùng cho thống kê và báo cáo admin
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian đăng nhập lần cuối
        /// Nullable - có thể null nếu chưa đăng nhập lần nào
        /// Dùng để tracking hoạt động của user và security
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// URL ảnh đại diện của người dùng
        /// Có thể null nếu không có ảnh đại diện
        /// Tối đa 255 ký tự để lưu đường dẫn
        /// </summary>
        [MaxLength(255)]
        public string? ProfileImageUrl { get; set; }

        // Navigation Properties - Quan hệ với các entity khác

        /// <summary>
        /// Danh sách các folder mà user tạo ra
        /// Một user có thể có nhiều folder để tổ chức bộ từ vựng
        /// </summary>
        public ICollection<Folder> Folders { get; set; }

        /// <summary>
        /// Lịch sử hoạt động của user trong hệ thống
        /// Lưu lại tất cả actions để audit và security
        /// </summary>
        public ICollection<AuditLog> AuditLogs { get; set; }

        /// <summary>
        /// Danh sách lịnh nhắc ôn tập mà user đã đặt
        /// Hỗ trợ tính năng spaced repetition learning
        /// </summary>
        public ICollection<ReviewReminder> ReviewReminders { get; set; }

        /// <summary>
        /// Lịch sử học tập của user với từng từ vựng
        /// Tracking progress và performance của user
        /// </summary>
        public ICollection<VocaItemHistory> VocaItemHistories { get; set; }

        /// <summary>
        /// Danh sách bộ từ vựng mà user tạo ra
        /// Quan hệ một-nhiều: một user có nhiều VocaSet
        /// </summary>
        public ICollection<VocaSet> VocaSets { get; set; }

        /// <summary>
        /// Danh sách các bộ từ vựng mà user đã copy từ người khác
        /// Tracking việc sử dụng nội dung public
        /// </summary>
        public ICollection<VocaSetCopy> VocaSetCopies { get; set; }

        /// <summary>
        /// Cài đặt cá nhân của user
        /// Quan hệ một-một với UserSettings
        /// </summary>
        public UserSettings? UserSettings { get; set; }
    }
}