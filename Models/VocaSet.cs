using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace VocaWebApp.Models
{
    /// <summary>
    /// Model đại diện cho một bộ từ vựng
    /// Là thành phần chính trong hệ thống học từ vựng
    /// </summary>
    public class VocaSet
    {
        /// <summary>
        /// ID duy nhất của bộ từ vựng
        /// Primary key, tự động tăng
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID của người dùng sở hữu bộ từ vựng
        /// Foreign key tới ApplicationUser
        /// Bắt buộc phải có - mỗi bộ từ phải thuộc về một user
        /// </summary>
        [Required]
        [ValidateNever]
        public string UserId { get; set; }

        /// <summary>
        /// Navigation property tới người dùng sở hữu
        /// Dùng để truy cập thông tin user từ VocaSet
        /// </summary>
        [ValidateNever]
        public ApplicationUser User { get; set; }

        /// <summary>
        /// ID của folder chứa bộ từ vựng này
        /// Nullable - có thể không thuộc folder nào (để ở root)
        /// Dùng để tổ chức phân cấp bộ từ vựng
        /// </summary>
        public int? FolderId { get; set; }

        /// <summary>
        /// Navigation property tới folder chứa
        /// Null nếu bộ từ không thuộc folder nào
        /// </summary>
        public Folder? Folder { get; set; }

        /// <summary>
        /// Tên của bộ từ vựng
        /// Bắt buộc nhập, tối đa 100 ký tự
        /// Hiển thị trong danh sách và tìm kiếm
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Mô tả chi tiết về bộ từ vựng
        /// Giúp user hiểu rõ nội dung và mục đích của bộ từ
        /// Hiển thị trong trang chi tiết và khi chia sẻ
        /// </summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Từ khóa liên quan đến bộ từ vựng
        /// Dùng cho tìm kiếm và phân loại
        /// Có thể chứa nhiều từ khóa cách nhau bởi dấu phẩy
        /// </summary>
        [MaxLength(255)]
        public string? Keywords { get; set; }

        /// <summary>
        /// Trạng thái chia sẻ của bộ từ vựng
        /// - "private": chỉ user tạo mới xem được
        /// - "public-view": mọi người có thể xem nhưng không copy
        /// - "public-copy": mọi người có thể xem và copy
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "private";

        /// <summary>
        /// Đánh dấu bộ từ vựng đã bị xóa mềm
        /// true = đã xóa, false = chưa xóa
        /// Dùng cho soft delete - không xóa khỏi database
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Đánh dấu bộ từ vựng bị ẩn bởi admin
        /// true = bị ẩn (vi phạm), false = hiển thị bình thường
        /// Admin dùng để kiểm duyệt nội dung
        /// </summary>
        public bool IsHidden { get; set; } = false;

        /// <summary>
        /// Số lượt xem bộ từ vựng
        /// Tăng mỗi khi có user truy cập xem chi tiết
        /// Dùng để đánh giá độ phổ biến và ranking
        /// </summary>
        public int ViewCount { get; set; } = 0;

        /// <summary>
        /// Thời gian truy cập lần cuối
        /// Nullable - có thể null nếu chưa truy cập lần nào
        /// Dùng để sắp xếp theo "gần đây nhất"
        /// </summary>
        public DateTime? LastAccessed { get; set; }

        /// <summary>
        /// Thời gian tạo bộ từ vựng
        /// Tự động gán khi tạo mới
        /// Dùng để sắp xếp và thống kê
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties - Quan hệ với các entity khác

        /// <summary>
        /// Danh sách các từ vựng trong bộ từ này
        /// Quan hệ một-nhiều: một VocaSet có nhiều VocaItem
        /// </summary>
        [ValidateNever]
        public ICollection<VocaItem> VocaItems { get; set; }

        /// <summary>
        /// Danh sách lịch nhắc ôn tập cho bộ từ này
        /// User có thể đặt nhiều lịch nhắc khác nhau
        /// </summary>
        [ValidateNever]
        public ICollection<ReviewReminder> ReviewReminders { get; set; }

        /// <summary>
        /// Danh sách các lần copy bộ từ này
        /// Tracking xem ai đã copy bộ từ của mình
        /// </summary>
        [ValidateNever]
        public ICollection<VocaSetCopy> Copies { get; set; }
    }
}