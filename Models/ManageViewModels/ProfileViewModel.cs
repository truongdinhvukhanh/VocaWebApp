using System.ComponentModel.DataAnnotations;

namespace VocaWebApp.Models.ManageViewModels
{
    /// <summary>
    /// ViewModel cho trang quản lý thông tin cá nhân
    /// Bao gồm cả thông tin từ ApplicationUser và UserSettings
    /// </summary>
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Ảnh đại diện hiện tại")]
        public string? CurrentProfileImageUrl { get; set; }

        [Display(Name = "Ảnh đại diện mới")]
        public string? NewProfileImageUrl { get; set; }

        // Thông tin từ ApplicationUser
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Thông tin từ UserSettings
        public int DailyGoal { get; set; } = 10;
        public bool EmailNotifications { get; set; } = true;
        public bool WebNotifications { get; set; } = true;
        public string PreferredLanguage { get; set; } = "vi";
        public string Theme { get; set; } = "light";
        public bool AutoPlayAudio { get; set; } = false;
        public int DefaultReviewInterval { get; set; } = 7;
        public int FlashcardSessionSize { get; set; } = 20;
        public bool ShowPronunciation { get; set; } = true;
        public bool ShowWordType { get; set; } = true;
        public string DefaultFlashcardMode { get; set; } = "word-to-meaning";
    }
}
