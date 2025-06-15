using Microsoft.EntityFrameworkCore;
using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    /// <summary>
    /// Implementation của IReviewReminderRepository sử dụng Entity Framework Core
    /// Triển khai các phương thức quản lý ReviewReminder với ApplicationDbContext
    /// </summary>
    public class ReviewReminderRepository : IReviewReminderRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor khởi tạo repository với ApplicationDbContext
        /// </summary>
        /// <param name="context">ApplicationDbContext để truy cập cơ sở dữ liệu</param>
        public ReviewReminderRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ========== CRUD Operations ==========

        /// <summary>
        /// Tạo mới một reminder để nhắc nhở ôn tập
        /// Thêm reminder vào DbContext và lưu thay đổi vào cơ sở dữ liệu
        /// </summary>
        /// <param name="reviewReminder">Đối tượng ReviewReminder cần tạo</param>
        /// <returns>ReviewReminder đã được tạo với ID được gán</returns>
        public async Task<ReviewReminder> CreateAsync(ReviewReminder reviewReminder)
        {
            if (reviewReminder == null)
                throw new ArgumentNullException(nameof(reviewReminder));

            _context.ReviewReminders.Add(reviewReminder);
            await _context.SaveChangesAsync();
            return reviewReminder;
        }

        /// <summary>
        /// Cập nhật thông tin của một reminder
        /// Đánh dấu entity đã được sửa đổi và lưu thay đổi
        /// </summary>
        /// <param name="reviewReminder">Đối tượng ReviewReminder cần cập nhật</param>
        /// <returns>ReviewReminder đã được cập nhật</returns>
        public async Task<ReviewReminder> UpdateAsync(ReviewReminder reviewReminder)
        {
            if (reviewReminder == null)
                throw new ArgumentNullException(nameof(reviewReminder));

            _context.ReviewReminders.Update(reviewReminder);
            await _context.SaveChangesAsync();
            return reviewReminder;
        }

        /// <summary>
        /// Xóa một reminder khỏi cơ sở dữ liệu
        /// Tìm reminder theo ID và xóa nếu tồn tại
        /// </summary>
        /// <param name="id">ID của reminder cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var reminder = await _context.ReviewReminders.FindAsync(id);
            if (reminder == null)
                return false;

            _context.ReviewReminders.Remove(reminder);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một reminder theo ID
        /// Sử dụng FindAsync để truy vấn nhanh theo primary key
        /// </summary>
        /// <param name="id">ID của reminder</param>
        /// <returns>ReviewReminder nếu tìm thấy, null nếu không tìm thấy</returns>
        public async Task<ReviewReminder?> GetByIdAsync(int id)
        {
            return await _context.ReviewReminders.FindAsync(id);
        }

        // ========== Query Operations ==========

        /// <summary>
        /// Lấy tất cả reminder của một user cụ thể
        /// Sắp xếp theo ReviewDate tăng dần
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Danh sách các ReviewReminder của user</returns>
        public async Task<IEnumerable<ReviewReminder>> GetByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _context.ReviewReminders
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.ReviewDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy tất cả reminder của một bộ từ vựng cụ thể
        /// Sắp xếp theo ReviewDate tăng dần
        /// </summary>
        /// <param name="vocaSetId">ID của bộ từ vựng</param>
        /// <returns>Danh sách các ReviewReminder của bộ từ vựng</returns>
        public async Task<IEnumerable<ReviewReminder>> GetByVocaSetIdAsync(int vocaSetId)
        {
            return await _context.ReviewReminders
                .Where(r => r.VocaSetId == vocaSetId)
                .OrderBy(r => r.ReviewDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy reminder của một user cho một bộ từ vựng cụ thể
        /// Sử dụng để kiểm tra hoặc lấy reminder đã tồn tại
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="vocaSetId">ID của bộ từ vựng</param>
        /// <returns>ReviewReminder nếu tìm thấy, null nếu không tìm thấy</returns>
        public async Task<ReviewReminder?> GetByUserAndVocaSetAsync(string userId, int vocaSetId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _context.ReviewReminders
                .FirstOrDefaultAsync(r => r.UserId == userId && r.VocaSetId == vocaSetId);
        }

        /// <summary>
        /// Lấy danh sách reminder cần gửi thông báo (chưa gửi và đã đến thời gian)
        /// Dùng cho background service xử lý thông báo
        /// </summary>
        /// <param name="currentTime">Thời gian hiện tại để so sánh</param>
        /// <returns>Danh sách các ReviewReminder cần gửi thông báo</returns>
        public async Task<IEnumerable<ReviewReminder>> GetPendingRemindersAsync(DateTime currentTime)
        {
            return await _context.ReviewReminders
                .Where(r => !r.IsSent && r.ReviewDate <= currentTime)
                .OrderBy(r => r.ReviewDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách reminder cần gửi email (IsEmail = true, IsSent = false, đã đến thời gian)
        /// Dùng cho email service gửi thông báo qua email
        /// </summary>
        /// <param name="currentTime">Thời gian hiện tại để so sánh</param>
        /// <returns>Danh sách các ReviewReminder cần gửi email</returns>
        public async Task<IEnumerable<ReviewReminder>> GetPendingEmailRemindersAsync(DateTime currentTime)
        {
            return await _context.ReviewReminders
                .Include(r => r.User)      // Load thông tin user để gửi email
                .Include(r => r.VocaSet)   // Load thông tin VocaSet để hiển thị trong email
                .Where(r => r.IsEmail && !r.IsSent && r.ReviewDate <= currentTime)
                .OrderBy(r => r.ReviewDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách reminder cần hiển thị thông báo web (IsNotification = true, IsSent = false, đã đến thời gian)
        /// Dùng cho web notification service hiển thị thông báo trên web
        /// </summary>
        /// <param name="currentTime">Thời gian hiện tại để so sánh</param>
        /// <returns>Danh sách các ReviewReminder cần hiển thị thông báo web</returns>
        public async Task<IEnumerable<ReviewReminder>> GetPendingWebNotificationRemindersAsync(DateTime currentTime)
        {
            return await _context.ReviewReminders
                .Include(r => r.VocaSet)   // Load thông tin VocaSet để hiển thị trong notification
                .Where(r => r.IsNotification && !r.IsSent && r.ReviewDate <= currentTime)
                .OrderBy(r => r.ReviewDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách reminder trong khoảng thời gian cụ thể
        /// Dùng cho báo cáo và thống kê reminder theo thời gian
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns>Danh sách các ReviewReminder trong khoảng thời gian</returns>
        public async Task<IEnumerable<ReviewReminder>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.ReviewReminders
                .Where(r => r.ReviewDate >= startDate && r.ReviewDate <= endDate)
                .OrderBy(r => r.ReviewDate)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách reminder của user trong khoảng thời gian cụ thể
        /// Dùng cho lịch ôn tập cá nhân của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns>Danh sách các ReviewReminder của user trong khoảng thời gian</returns>
        public async Task<IEnumerable<ReviewReminder>> GetByUserAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _context.ReviewReminders
                .Where(r => r.UserId == userId && r.ReviewDate >= startDate && r.ReviewDate <= endDate)
                .OrderBy(r => r.ReviewDate)
                .ToListAsync();
        }

        // ========== Status Management ==========

        /// <summary>
        /// Đánh dấu một reminder đã được gửi thông báo
        /// Cập nhật IsSent = true và lưu thay đổi
        /// </summary>
        /// <param name="id">ID của reminder</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy</returns>
        public async Task<bool> MarkAsSentAsync(int id)
        {
            var reminder = await _context.ReviewReminders.FindAsync(id);
            if (reminder == null)
                return false;

            reminder.IsSent = true;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Đánh dấu nhiều reminder đã được gửi thông báo
        /// Sử dụng bulk update để cập nhật hiệu quả nhiều record
        /// </summary>
        /// <param name="ids">Danh sách ID của các reminder</param>
        /// <returns>Số lượng reminder đã được cập nhật thành công</returns>
        public async Task<int> MarkMultipleAsSentAsync(IEnumerable<int> ids)
        {
            if (ids == null || !ids.Any())
                return 0;

            var reminders = await _context.ReviewReminders
                .Where(r => ids.Contains(r.Id))
                .ToListAsync();

            foreach (var reminder in reminders)
            {
                reminder.IsSent = true;
            }

            await _context.SaveChangesAsync();
            return reminders.Count;
        }

        /// <summary>
        /// Đặt lại trạng thái chưa gửi cho một reminder
        /// Cập nhật IsSent = false, dùng khi cần gửi lại thông báo
        /// </summary>
        /// <param name="id">ID của reminder</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy</returns>
        public async Task<bool> ResetSentStatusAsync(int id)
        {
            var reminder = await _context.ReviewReminders.FindAsync(id);
            if (reminder == null)
                return false;

            reminder.IsSent = false;
            await _context.SaveChangesAsync();
            return true;
        }

        // ========== Advanced Operations ==========

        /// <summary>
        /// Tự động tạo reminder tiếp theo cho các reminder có RepeatIntervalDays
        /// Dùng cho chức năng lặp lại thông báo theo chu kỳ
        /// </summary>
        /// <param name="reminderId">ID của reminder gốc</param>
        /// <returns>ReviewReminder mới được tạo nếu có RepeatIntervalDays, null nếu không</returns>
        public async Task<ReviewReminder?> CreateNextRecurringReminderAsync(int reminderId)
        {
            var originalReminder = await _context.ReviewReminders.FindAsync(reminderId);
            if (originalReminder == null || !originalReminder.RepeatIntervalDays.HasValue)
                return null;

            var nextReminder = new ReviewReminder
            {
                UserId = originalReminder.UserId,
                VocaSetId = originalReminder.VocaSetId,
                ReviewDate = originalReminder.ReviewDate.AddDays(originalReminder.RepeatIntervalDays.Value),
                RepeatIntervalDays = originalReminder.RepeatIntervalDays,
                IsEmail = originalReminder.IsEmail,
                IsNotification = originalReminder.IsNotification,
                IsSent = false
            };

            _context.ReviewReminders.Add(nextReminder);
            await _context.SaveChangesAsync();
            return nextReminder;
        }

        /// <summary>
        /// Kiểm tra xem user đã có reminder cho bộ từ vựng này chưa
        /// Dùng để tránh tạo trùng lặp reminder
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="vocaSetId">ID của bộ từ vựng</param>
        /// <returns>True nếu đã có reminder, False nếu chưa có</returns>
        public async Task<bool> ExistsAsync(string userId, int vocaSetId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _context.ReviewReminders
                .AnyAsync(r => r.UserId == userId && r.VocaSetId == vocaSetId);
        }

        /// <summary>
        /// Đếm số lượng reminder chưa gửi của một user
        /// Dùng cho dashboard hiển thị thống kê reminder
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Số lượng reminder chưa gửi</returns>
        public async Task<int> CountPendingByUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            return await _context.ReviewReminders
                .CountAsync(r => r.UserId == userId && !r.IsSent);
        }

        /// <summary>
        /// Lấy danh sách reminder kèm thông tin user và VocaSet (với Include)
        /// Dùng cho các trang hiển thị chi tiết reminder với thông tin liên quan
        /// </summary>
        /// <param name="userId">ID của user (có thể null để lấy tất cả)</param>
        /// <returns>Danh sách ReviewReminder kèm thông tin liên quan</returns>
        public async Task<IEnumerable<ReviewReminder>> GetWithDetailsAsync(string? userId = null)
        {
            var query = _context.ReviewReminders
                .Include(r => r.User)      // Load thông tin user
                .Include(r => r.VocaSet)   // Load thông tin VocaSet
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(r => r.UserId == userId);
            }

            return await query
                .OrderBy(r => r.ReviewDate)
                .ToListAsync();
        }
    }
}