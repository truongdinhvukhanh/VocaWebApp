using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VocaWebApp.Models
{
    /// <summary>
    /// Model lưu trữ lịch sử học tập của user với từng từ vựng
    /// Dùng để tracking progress và áp dụng thuật toán spaced repetition
    /// </summary>
    public class VocaItemHistory
    {
        /// <summary>
        /// ID duy nhất của bản ghi lịch sử
        /// Primary key, tự động tăng
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID của người dùng học từ vựng
        /// Foreign key tới ApplicationUser
        /// Bắt buộc - mỗi history phải thuộc về một user
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// Navigation property tới người dùng
        /// Dùng để truy cập thông tin user từ VocaItemHistory
        /// </summary>
        public ApplicationUser User { get; set; }

        /// <summary>
        /// ID của từ vựng được học
        /// Foreign key tới VocaItem
        /// Bắt buộc - mỗi history phải liên kết với một từ vựng
        /// </summary>
        [Required]
        public int VocaItemId { get; set; }

        /// <summary>
        /// Navigation property tới từ vựng
        /// Dùng để truy cập thông tin VocaItem từ history
        /// </summary>
        public VocaItem VocaItem { get; set; }

        /// <summary>
        /// Trạng thái học tập trong lần ôn này
        /// Bắt buộc nhập, tối đa 20 ký tự
        /// "learned" = đã thuộc, "notlearned" = chưa thuộc
        /// Dùng để đánh giá hiệu quả học tập
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        /// <summary>
        /// Thời gian ôn tập/học từ vựng
        /// Tự động gán khi tạo record mới
        /// Dùng để tracking frequency và timing
        /// </summary>
        public DateTime ReviewedAt { get; set; } = DateTime.UtcNow;
    }
}