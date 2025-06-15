using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VocaWebApp.Models
{
    /// <summary>
    /// Model đại diện cho thư mục chứa các bộ từ vựng
    /// Giúp người dùng tổ chức và phân loại bộ từ vựng
    /// </summary>
    public class Folder
    {
        /// <summary>
        /// ID duy nhất của folder
        /// Primary key, tự động tăng
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID của người dùng sở hữu folder này
        /// Foreign key tới ApplicationUser
        /// Bắt buộc - mỗi folder phải thuộc về một user
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Navigation property tới người dùng sở hữu
        /// Dùng để truy cập thông tin user từ Folder
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// Tên của folder
        /// Bắt buộc nhập, tối đa 100 ký tự
        /// Hiển thị trong danh sách folder và breadcrumb
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Mô tả chi tiết về folder
        /// Bắt buộc nhập, tối đa 1000 ký tự
        /// Giúp user nhớ mục đích và nội dung của folder
        /// </summary>
        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        /// <summary>
        /// Thời gian tạo folder
        /// Tự động gán khi tạo folder mới
        /// Dùng để sắp xếp theo thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Danh sách các bộ từ vựng trong folder này
        /// Quan hệ một-nhiều: một folder có nhiều VocaSet
        /// Dùng để hiển thị nội dung folder
        /// </summary>
        public ICollection<VocaSet> VocaSets { get; set; }
    }
}