using System.ComponentModel.DataAnnotations;

namespace VocaWebApp.Models
{
    /// <summary>
    /// Model lưu trữ lịch sử hoạt động của người dùng trong hệ thống
    /// Dùng cho mục đích audit, security và debug
    /// </summary>
    public class AuditLog
    {
        /// <summary>
        /// ID duy nhất của bản ghi audit log
        /// Primary key, tự động tăng
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID của người dùng thực hiện hành động
        /// Nullable - có thể null nếu là hành động của system
        /// Foreign key tới ApplicationUser
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Navigation property tới người dùng
        /// Null nếu là system action
        /// Dùng để truy cập thông tin user từ log
        /// </summary>
        public ApplicationUser? User { get; set; }

        /// <summary>
        /// Mô tả hành động được thực hiện
        /// Bắt buộc nhập, tối đa 255 ký tự
        /// Ví dụ: "LOGIN", "CREATE_VOCASET", "DELETE_FOLDER"
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string Action { get; set; }

        /// <summary>
        /// Chi tiết bổ sung về hành động
        /// Có thể null nếu không cần thông tin thêm
        /// Lưu dữ liệu JSON hoặc text mô tả chi tiết
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Thời gian thực hiện hành động
        /// Tự động gán khi tạo log mới
        /// Dùng UTC để đảm bảo tính nhất quán
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}