using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace VocaWebApp.Models.ManageViewModels
{
    /// <summary>
    /// ViewModel cho trang quản lý thông tin cá nhân.
    /// Bao gồm cả thông tin từ ApplicationUser và UserSettings.
    /// </summary>
    public class ProfileViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Ảnh đại diện hiện tại")]
        public string? CurrentProfileImageUrl { get; set; }

        [Display(Name = "Tải ảnh đại diện mới")]
        public IFormFile? AvatarFile { get; set; }

        // Thông tin từ ApplicationUser
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Thông tin từ UserSettings
        [Display(Name = "Mục tiêu hàng ngày")]
        public int DailyGoal { get; set; } = 10;

        [Display(Name = "Nhận thông báo qua Email")]
        public bool EmailNotifications { get; set; } = true;

        [Display(Name = "Hiển thị thông báo trên Web")]
        public bool WebNotifications { get; set; } = true;

        [Display(Name = "Ngôn ngữ ưu tiên")]
        public string PreferredLanguage { get; set; } = "vi";

        [Display(Name = "Giao diện")]
        public string Theme { get; set; } = "light";

        [Display(Name = "Tự động phát âm thanh")]
        public bool AutoPlayAudio { get; set; } = false;

        [Display(Name = "Khoảng cách ôn tập mặc định (ngày)")]
        public int DefaultReviewInterval { get; set; } = 7;

        [Display(Name = "Số từ trong một phiên Flashcard")]
        public int FlashcardSessionSize { get; set; } = 20;

        [Display(Name = "Hiển thị phiên âm")]
        public bool ShowPronunciation { get; set; } = true;

        [Display(Name = "Hiển thị từ loại")]
        public bool ShowWordType { get; set; } = true;

        [Display(Name = "Chế độ Flashcard mặc định")]
        public string DefaultFlashcardMode { get; set; } = "word-to-meaning";
    }
}
