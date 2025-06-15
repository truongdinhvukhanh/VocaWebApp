using System.ComponentModel.DataAnnotations;

namespace VocaWebApp.Models.ManageViewModels
{
    /// <summary>
    /// ViewModel cho trang quản lý thông tin cá nhân
    /// Bao gồm các trường thông tin người dùng có thể cập nhật
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

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Ảnh đại diện hiện tại")]
        public string CurrentProfileImageUrl { get; set; }

        [Display(Name = "Ảnh đại diện mới")]
        public string NewProfileImageUrl { get; set; }
    }
}