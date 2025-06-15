using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VocaWebApp.Models
{
    /// <summary>
    /// Model lưu trữ thông tin về việc copy bộ từ vựng public
    /// Tracking việc sử dụng và chia sẻ nội dung giữa các user
    /// </summary>
    public class VocaSetCopy
    {
        /// <summary>
        /// ID duy nhất của bản copy
        /// Primary key, tự động tăng
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID của bộ từ vựng gốc được copy
        /// Foreign key tới VocaSet
        /// Bắt buộc - phải xác định được bộ từ gốc
        /// </summary>
        [Required]
        public int OriginalSetId { get; set; }

        /// <summary>
        /// Navigation property tới bộ từ vựng gốc
        /// Dùng để truy cập thông tin của bộ từ được copy
        /// </summary>
        public VocaSet OriginalSet { get; set; }

        /// <summary>
        /// ID của người dùng thực hiện copy
        /// Foreign key tới ApplicationUser
        /// Bắt buộc - phải biết ai là người copy
        /// </summary>
        [Required]
        public string CopiedByUserId { get; set; }

        /// <summary>
        /// Navigation property tới người copy
        /// Dùng để truy cập thông tin user thực hiện copy
        /// </summary>
        public ApplicationUser CopiedByUser { get; set; }

        /// <summary>
        /// Thời gian thực hiện copy
        /// Tự động gán khi tạo record mới
        /// Dùng để tracking và thống kê việc chia sẻ
        /// </summary>
        public DateTime CopiedAt { get; set; } = DateTime.UtcNow;
    }
}