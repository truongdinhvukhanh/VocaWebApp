using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    /// <summary>
    /// Interface định nghĩa các phương thức để quản lý ReviewReminder trong cơ sở dữ liệu
    /// Cung cấp các thao tác CRUD và truy vấn chuyên biệt cho việc nhắc nhở ôn tập
    /// </summary>
    public interface IReviewReminderRepository
    {
        // ========== CRUD Operations ==========

        /// <summary>
        /// Tạo mới một reminder để nhắc nhở ôn tập
        /// </summary>
        /// <param name="reviewReminder">Đối tượng ReviewReminder cần tạo</param>
        /// <returns>ReviewReminder đã được tạo với ID được gán</returns>
        Task<ReviewReminder> CreateAsync(ReviewReminder reviewReminder);

        /// <summary>
        /// Cập nhật thông tin của một reminder
        /// </summary>
        /// <param name="reviewReminder">Đối tượng ReviewReminder cần cập nhật</param>
        /// <returns>ReviewReminder đã được cập nhật</returns>
        Task<ReviewReminder> UpdateAsync(ReviewReminder reviewReminder);

        /// <summary>
        /// Xóa một reminder khỏi cơ sở dữ liệu
        /// </summary>
        /// <param name="id">ID của reminder cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Lấy thông tin chi tiết của một reminder theo ID
        /// </summary>
        /// <param name="id">ID của reminder</param>
        /// <returns>ReviewReminder nếu tìm thấy, null nếu không tìm thấy</returns>
        Task<ReviewReminder?> GetByIdAsync(int id);

        // ========== Query Operations ==========

        /// <summary>
        /// Lấy tất cả reminder của một user cụ thể
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Danh sách các ReviewReminder của user</returns>
        Task<IEnumerable<ReviewReminder>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Lấy tất cả reminder của một bộ từ vựng cụ thể
        /// </summary>
        /// <param name="vocaSetId">ID của bộ từ vựng</param>
        /// <returns>Danh sách các ReviewReminder của bộ từ vựng</returns>
        Task<IEnumerable<ReviewReminder>> GetByVocaSetIdAsync(int vocaSetId);

        /// <summary>
        /// Lấy reminder của một user cho một bộ từ vựng cụ thể
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="vocaSetId">ID của bộ từ vựng</param>
        /// <returns>ReviewReminder nếu tìm thấy, null nếu không tìm thấy</returns>
        Task<ReviewReminder?> GetByUserAndVocaSetAsync(string userId, int vocaSetId);

        /// <summary>
        /// Lấy danh sách reminder cần gửi thông báo (chưa gửi và đã đến thời gian)
        /// </summary>
        /// <param name="currentTime">Thời gian hiện tại để so sánh</param>
        /// <returns>Danh sách các ReviewReminder cần gửi thông báo</returns>
        Task<IEnumerable<ReviewReminder>> GetPendingRemindersAsync(DateTime currentTime);

        /// <summary>
        /// Lấy danh sách reminder cần gửi email (IsEmail = true, IsSent = false, đã đến thời gian)
        /// </summary>
        /// <param name="currentTime">Thời gian hiện tại để so sánh</param>
        /// <returns>Danh sách các ReviewReminder cần gửi email</returns>
        Task<IEnumerable<ReviewReminder>> GetPendingEmailRemindersAsync(DateTime currentTime);

        /// <summary>
        /// Lấy danh sách reminder cần hiển thị thông báo web (IsNotification = true, IsSent = false, đã đến thời gian)
        /// </summary>
        /// <param name="currentTime">Thời gian hiện tại để so sánh</param>
        /// <returns>Danh sách các ReviewReminder cần hiển thị thông báo web</returns>
        Task<IEnumerable<ReviewReminder>> GetPendingWebNotificationRemindersAsync(DateTime currentTime);

        /// <summary>
        /// Lấy danh sách reminder trong khoảng thời gian cụ thể
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns>Danh sách các ReviewReminder trong khoảng thời gian</returns>
        Task<IEnumerable<ReviewReminder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Lấy danh sách reminder của user trong khoảng thời gian cụ thể
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns>Danh sách các ReviewReminder của user trong khoảng thời gian</returns>
        Task<IEnumerable<ReviewReminder>> GetByUserAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate);

        // ========== Status Management ==========

        /// <summary>
        /// Đánh dấu một reminder đã được gửi thông báo
        /// </summary>
        /// <param name="id">ID của reminder</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy</returns>
        Task<bool> MarkAsSentAsync(int id);

        /// <summary>
        /// Đánh dấu nhiều reminder đã được gửi thông báo
        /// </summary>
        /// <param name="ids">Danh sách ID của các reminder</param>
        /// <returns>Số lượng reminder đã được cập nhật thành công</returns>
        Task<int> MarkMultipleAsSentAsync(IEnumerable<int> ids);

        /// <summary>
        /// Đặt lại trạng thái chưa gửi cho một reminder
        /// </summary>
        /// <param name="id">ID của reminder</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy</returns>
        Task<bool> ResetSentStatusAsync(int id);

        // ========== Advanced Operations ==========

        /// <summary>
        /// Tự động tạo reminder tiếp theo cho các reminder có RepeatIntervalDays
        /// </summary>
        /// <param name="reminderId">ID của reminder gốc</param>
        /// <returns>ReviewReminder mới được tạo nếu có RepeatIntervalDays, null nếu không</returns>
        Task<ReviewReminder?> CreateNextRecurringReminderAsync(int reminderId);

        /// <summary>
        /// Kiểm tra xem user đã có reminder cho bộ từ vựng này chưa
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="vocaSetId">ID của bộ từ vựng</param>
        /// <returns>True nếu đã có reminder, False nếu chưa có</returns>
        Task<bool> ExistsAsync(string userId, int vocaSetId);

        /// <summary>
        /// Đếm số lượng reminder chưa gửi của một user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Số lượng reminder chưa gửi</returns>
        Task<int> CountPendingByUserAsync(string userId);

        /// <summary>
        /// Lấy danh sách reminder kèm thông tin user và VocaSet (với Include)
        /// </summary>
        /// <param name="userId">ID của user (có thể null để lấy tất cả)</param>
        /// <returns>Danh sách ReviewReminder kèm thông tin liên quan</returns>
        Task<IEnumerable<ReviewReminder>> GetWithDetailsAsync(string? userId = null);
    }
}