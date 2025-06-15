using Microsoft.EntityFrameworkCore;
using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    /// <summary>
    /// Repository để tương tác với bảng AuditLog trong cơ sở dữ liệu
    /// Cung cấp các phương thức để ghi lại, truy vấn và quản lý hoạt động của người dùng
    /// </summary>
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Khởi tạo repository với dependency injection
        /// </summary>
        /// <param name="context">DbContext để tương tác với cơ sở dữ liệu</param>
        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Phương thức ghi log cơ bản

        /// <summary>
        /// Ghi một bản ghi audit log mới vào cơ sở dữ liệu
        /// </summary>
        /// <param name="auditLog">Đối tượng AuditLog chứa thông tin hoạt động cần ghi lại</param>
        public async Task AddAsync(AuditLog auditLog)
        {
            if (auditLog == null)
                throw new ArgumentNullException(nameof(auditLog));

            // Đảm bảo rằng CreatedAt được thiết lập nếu chưa có
            if (auditLog.CreatedAt == default)
                auditLog.CreatedAt = DateTime.UtcNow;

            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Ghi một bản ghi audit log với thông tin chi tiết
        /// Phương thức tiện ích để tạo và ghi log nhanh chóng
        /// </summary>
        /// <param name="userId">ID của người dùng thực hiện hoạt động (null nếu là system action)</param>
        /// <param name="action">Mô tả hành động được thực hiện</param>
        /// <param name="details">Chi tiết bổ sung về hoạt động</param>
        public async Task LogAsync(string? userId, string action, string? details = null)
        {
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("Action không được để trống", nameof(action));

            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                CreatedAt = DateTime.UtcNow
            };

            await AddAsync(auditLog);
        }

        /// <summary>
        /// Ghi nhiều bản ghi audit log cùng lúc
        /// </summary>
        /// <param name="auditLogs">Danh sách các AuditLog cần ghi</param>
        public async Task AddRangeAsync(IEnumerable<AuditLog> auditLogs)
        {
            if (auditLogs == null)
                throw new ArgumentNullException(nameof(auditLogs));

            // Đảm bảo tất cả bản ghi có CreatedAt
            foreach (var log in auditLogs)
            {
                if (log.CreatedAt == default)
                    log.CreatedAt = DateTime.UtcNow;
            }

            await _context.AuditLogs.AddRangeAsync(auditLogs);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Phương thức truy vấn cơ bản

        /// <summary>
        /// Lấy một bản ghi audit log theo ID
        /// </summary>
        /// <param name="id">ID của bản ghi audit log</param>
        /// <returns>Đối tượng AuditLog nếu tìm thấy, null nếu không tìm thấy</returns>
        public async Task<AuditLog?> GetByIdAsync(int id)
        {
            return await _context.AuditLogs
                .Include(a => a.User)  // Eager loading navigation property
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <summary>
        /// Lấy tất cả các bản ghi audit log
        /// Chú ý: Chỉ sử dụng cho mục đích testing hoặc khi chắc chắn dữ liệu ít
        /// </summary>
        /// <returns>Danh sách tất cả AuditLog trong hệ thống</returns>
        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        #endregion

        #region Phương thức truy vấn theo điều kiện

        /// <summary>
        /// Lấy tất cả các bản ghi audit log của một người dùng cụ thể
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Danh sách AuditLog của người dùng được sắp xếp theo thời gian mới nhất</returns>
        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId không được để trống", nameof(userId));

            return await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy các bản ghi audit log theo loại hành động
        /// </summary>
        /// <param name="action">Tên hành động cần tìm kiếm</param>
        /// <returns>Danh sách AuditLog có cùng loại hành động</returns>
        public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
        {
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("Action không được để trống", nameof(action));

            return await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.Action == action)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy các bản ghi audit log trong khoảng thời gian nhất định
        /// </summary>
        /// <param name="fromDate">Thời gian bắt đầu</param>
        /// <param name="toDate">Thời gian kết thúc</param>
        /// <returns>Danh sách AuditLog trong khoảng thời gian chỉ định</returns>
        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
                throw new ArgumentException("Thời gian bắt đầu phải trước thời gian kết thúc");

            return await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.CreatedAt >= fromDate && a.CreatedAt <= toDate)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy các bản ghi audit log của một người dùng trong khoảng thời gian nhất định
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="fromDate">Thời gian bắt đầu</param>
        /// <param name="toDate">Thời gian kết thúc</param>
        /// <returns>Danh sách AuditLog của user trong khoảng thời gian</returns>
        public async Task<IEnumerable<AuditLog>> GetByUserAndDateRangeAsync(string userId, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId không được để trống", nameof(userId));

            if (fromDate > toDate)
                throw new ArgumentException("Thời gian bắt đầu phải trước thời gian kết thúc");

            return await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.UserId == userId && a.CreatedAt >= fromDate && a.CreatedAt <= toDate)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        #endregion

        #region Phương thức truy vấn với phân trang

        /// <summary>
        /// Lấy danh sách audit log có phân trang
        /// </summary>
        /// <param name="pageNumber">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số bản ghi trên mỗi trang</param>
        /// <returns>Danh sách AuditLog được phân trang</returns>
        public async Task<IEnumerable<AuditLog>> GetPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Số trang phải lớn hơn hoặc bằng 1", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Kích thước trang phải lớn hơn hoặc bằng 1", nameof(pageSize));

            return await _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách audit log của một user có phân trang
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="pageNumber">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số bản ghi trên mỗi trang</param>
        /// <returns>Danh sách AuditLog của user được phân trang</returns>
        public async Task<IEnumerable<AuditLog>> GetPagedByUserAsync(string userId, int pageNumber, int pageSize)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId không được để trống", nameof(userId));

            if (pageNumber < 1)
                throw new ArgumentException("Số trang phải lớn hơn hoặc bằng 1", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Kích thước trang phải lớn hơn hoặc bằng 1", nameof(pageSize));

            return await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        #endregion

        #region Phương thức tìm kiếm và thống kê

        /// <summary>
        /// Tìm kiếm audit log theo từ khóa trong Action hoặc Details
        /// </summary>
        /// <param name="keyword">Từ khóa cần tìm kiếm</param>
        /// <returns>Danh sách AuditLog chứa từ khóa</returns>
        public async Task<IEnumerable<AuditLog>> SearchAsync(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
                throw new ArgumentException("Từ khóa tìm kiếm không được để trống", nameof(keyword));

            return await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.Action.Contains(keyword) ||
                           (a.Details != null && a.Details.Contains(keyword)))
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Đếm tổng số bản ghi audit log
        /// </summary>
        /// <returns>Tổng số bản ghi trong bảng AuditLog</returns>
        public async Task<int> GetTotalCountAsync()
        {
            return await _context.AuditLogs.CountAsync();
        }

        /// <summary>
        /// Đếm số bản ghi audit log của một người dùng
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Tổng số hoạt động của người dùng</returns>
        public async Task<int> GetCountByUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId không được để trống", nameof(userId));

            return await _context.AuditLogs
                .CountAsync(a => a.UserId == userId);
        }

        /// <summary>
        /// Lấy các hoạt động mới nhất của hệ thống
        /// </summary>
        /// <param name="limit">Số lượng bản ghi mới nhất cần lấy</param>
        /// <returns>Danh sách AuditLog mới nhất</returns>
        public async Task<IEnumerable<AuditLog>> GetRecentActivitiesAsync(int limit = 10)
        {
            if (limit < 1)
                throw new ArgumentException("Limit phải lớn hơn hoặc bằng 1", nameof(limit));

            return await _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy thống kê số lượng hoạt động theo ngày
        /// </summary>
        /// <param name="days">Số ngày gần đây cần thống kê</param>
        /// <returns>Dictionary với key là ngày, value là số lượng hoạt động</returns>
        public async Task<Dictionary<DateTime, int>> GetDailyActivityStatsAsync(int days = 7)
        {
            if (days < 1)
                throw new ArgumentException("Số ngày phải lớn hơn hoặc bằng 1", nameof(days));

            var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);
            var endDate = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1); // Đến cuối ngày hôm nay

            var logs = await _context.AuditLogs
                .Where(a => a.CreatedAt >= startDate && a.CreatedAt <= endDate)
                .ToListAsync();

            var result = new Dictionary<DateTime, int>();

            // Khởi tạo tất cả các ngày trong khoảng thời gian với giá trị 0
            for (var date = startDate; date <= endDate.Date; date = date.AddDays(1))
            {
                result[date] = 0;
            }

            // Đếm số lượng logs cho mỗi ngày
            foreach (var log in logs)
            {
                var day = log.CreatedAt.Date;
                result[day] = result[day] + 1;
            }

            return result;
        }

        /// <summary>
        /// Lấy thống kê các loại hoạt động phổ biến
        /// </summary>
        /// <param name="limit">Số lượng loại hoạt động top cần lấy</param>
        /// <returns>Dictionary với key là tên action, value là số lần xuất hiện</returns>
        public async Task<Dictionary<string, int>> GetTopActionsAsync(int limit = 10)
        {
            if (limit < 1)
                throw new ArgumentException("Limit phải lớn hơn hoặc bằng 1", nameof(limit));

            var actionGroups = await _context.AuditLogs
                .GroupBy(a => a.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(limit)
                .ToListAsync();

            return actionGroups.ToDictionary(g => g.Action, g => g.Count);
        }

        #endregion

        #region Phương thức quản lý dữ liệu

        /// <summary>
        /// Xóa các bản ghi audit log cũ hơn số ngày chỉ định
        /// </summary>
        /// <param name="days">Số ngày, các bản ghi cũ hơn sẽ bị xóa</param>
        /// <returns>Số lượng bản ghi đã được xóa</returns>
        public async Task<int> DeleteOldRecordsAsync(int days)
        {
            if (days < 1)
                throw new ArgumentException("Số ngày phải lớn hơn hoặc bằng 1", nameof(days));

            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            var oldLogs = await _context.AuditLogs
                .Where(a => a.CreatedAt < cutoffDate)
                .ToListAsync();

            if (oldLogs.Any())
            {
                _context.AuditLogs.RemoveRange(oldLogs);
                await _context.SaveChangesAsync();
            }

            return oldLogs.Count;
        }

        /// <summary>
        /// Kiểm tra xem có bản ghi nào tồn tại không
        /// </summary>
        /// <returns>True nếu có ít nhất một bản ghi, False nếu bảng rỗng</returns>
        public async Task<bool> AnyAsync()
        {
            return await _context.AuditLogs.AnyAsync();
        }

        /// <summary>
        /// Lưu các thay đổi vào cơ sở dữ liệu
        /// </summary>
        /// <returns>Số lượng bản ghi đã được lưu</returns>
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        #endregion
    }
}