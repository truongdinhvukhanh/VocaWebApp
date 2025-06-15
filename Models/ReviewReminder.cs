using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VocaWebApp.Models
{
    /// <summary>
    /// Model lưu trữ lịch nhắc ôn tập cho bộ từ vựng
    /// Hỗ trợ tính năng spaced repetition learning
    /// </summary>
    public class ReviewReminder
    {
        /// <summary>
        /// ID duy nhất của lịch nhắc
        /// Primary key, tự động tăng
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID của người dùng đặt lịch nhắc
        /// Foreign key tới ApplicationUser
        /// Bắt buộc - mỗi reminder phải thuộc về một user
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Navigation property tới người dùng
        /// Dùng để truy cập thông tin user từ ReviewReminder
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// ID của bộ từ vựng cần ôn tập
        /// Foreign key tới VocaSet
        /// Bắt buộc - mỗi reminder phải liên kết với một bộ từ
        /// </summary>
        [Required]
        public int VocaSetId { get; set; }

        /// <summary>
        /// Navigation property tới bộ từ vựng
        /// Dùng để truy cập thông tin VocaSet từ reminder
        /// </summary>
        public VocaSet VocaSet { get; set; }

        /// <summary>
        /// Ngày giờ cần ôn tập
        /// Thời điểm hệ thống sẽ gửi thông báo nhắc nhở
        /// </summary>
        public DateTime ReviewDate { get; set; }

        /// <summary>
        /// Khoảng cách ngày giữa các lần nhắc lặp lại
        /// Nullable - null nếu chỉ nhắc một lần
        /// Dùng cho việc tạo lịch nhắc định kỳ
        /// </summary>
        public int? RepeatIntervalDays { get; set; }

        /// <summary>
        /// Có gửi thông báo qua email hay không
        /// true = gửi email, false = không gửi email
        /// Kết hợp với cài đặt email của user
        /// </summary>
        public bool IsEmail { get; set; } = false;

        /// <summary>
        /// Có hiển thị thông báo trên web hay không
        /// true = hiển thị notification, false = không hiển thị
        /// Dùng cho popup/toast notification
        /// </summary>
        public bool IsNotification { get; set; } = true;

        /// <summary>
        /// Đã gửi thông báo chưa
        /// true = đã gửi, false = chưa gửi
        /// Tránh gửi trùng lặp thông báo
        /// </summary>
        public bool IsSent { get; set; } = false;
    }
}