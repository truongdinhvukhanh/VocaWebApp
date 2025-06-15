using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VocaWebApp.Models
{
    /// <summary>
    /// Model lưu trữ các cài đặt cá nhân của người dùng
    /// Giúp cá nhân hóa trải nghiệm học tập và giao diện
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// ID duy nhất của bản ghi cài đặt
        /// Primary key, tự động tăng
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID của người dùng sở hữu cài đặt này
        /// Foreign key tới ApplicationUser
        /// Quan hệ một-một: mỗi user có một bản cài đặt
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Navigation property tới người dùng
        /// Dùng để truy cập thông tin user từ UserSettings
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Mục tiêu học từ mới mỗi ngày
        /// Số từ vựng user muốn học trong một ngày
        /// Dùng để tracking progress và motivation
        /// </summary>
        public int DailyGoal { get; set; } = 10;

        /// <summary>
        /// Có gửi email thông báo hay không
        /// true = gửi email nhắc nhở, false = không gửi
        /// Áp dụng cho các thông báo về lịch ôn tập
        /// </summary>
        public bool EmailNotifications { get; set; } = true;

        /// <summary>
        /// Có hiển thị thông báo trên web hay không
        /// true = hiển thị popup/toast notification
        /// false = không hiển thị thông báo trên web
        /// </summary>
        public bool WebNotifications { get; set; } = true;

        /// <summary>
        /// Ngôn ngữ ưa thích của người dùng
        /// Mã ngôn ngữ theo chuẩn ISO (vi, en, ja, ko,...)
        /// Dùng để hiển thị giao diện và hướng dẫn
        /// </summary>
        [MaxLength(10)]
        public string PreferredLanguage { get; set; } = "vi";

        /// <summary>
        /// Giao diện màu sắc ưa thích
        /// - "light": giao diện sáng
        /// - "dark": giao diện tối
        /// - "auto": tự động theo hệ thống
        /// </summary>
        [MaxLength(10)]
        public string Theme { get; set; } = "light";

        /// <summary>
        /// Có phát âm thanh tự động khi hiển thị từ không
        /// true = tự động phát âm, false = phải click mới phát
        /// Áp dụng trong chế độ flashcard và học từ
        /// </summary>
        public bool AutoPlayAudio { get; set; } = false;

        /// <summary>
        /// Thời gian mặc định cho interval ôn tập (tính bằng ngày)
        /// Khoảng cách ngày giữa các lần ôn tập
        /// Áp dụng thuật toán spaced repetition
        /// </summary>
        public int DefaultReviewInterval { get; set; } = 7;

        /// <summary>
        /// Số từ hiển thị trong một session flashcard
        /// Giới hạn số từ để tránh overwhelm user
        /// </summary>
        public int FlashcardSessionSize { get; set; } = 20;

        /// <summary>
        /// Có hiển thị phiên âm khi học từ không
        /// true = hiển thị pronunciation, false = ẩn
        /// Giúp user tùy chọn độ khó khi học
        /// </summary>
        public bool ShowPronunciation { get; set; } = true;

        /// <summary>
        /// Có hiển thị từ loại (word type) không
        /// true = hiển thị (noun, verb, adj...), false = ẩn
        /// </summary>
        public bool ShowWordType { get; set; } = true;

        /// <summary>
        /// Chế độ flashcard mặc định
        /// - "word-to-meaning": hiển thị từ, đoán nghĩa
        /// - "meaning-to-word": hiển thị nghĩa, đoán từ
        /// - "mixed": trộn lẫn cả hai chế độ
        /// </summary>
        [MaxLength(20)]
        public string DefaultFlashcardMode { get; set; } = "word-to-meaning";

        /// <summary>
        /// Thời gian tạo bản cài đặt
        /// Tự động gán khi tạo mới
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian cập nhật cài đặt lần cuối
        /// Tự động cập nhật khi user thay đổi settings
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}