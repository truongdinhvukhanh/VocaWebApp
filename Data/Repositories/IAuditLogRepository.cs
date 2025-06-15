using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    /// <summary>
    /// Interface định nghĩa các phương thức cần thiết để truy cập và quản lý dữ liệu AuditLog
    /// Cung cấp các chức năng ghi lại, truy vấn và quản lý lịch sử hoạt động của người dùng trong hệ thống
    /// </summary>
    public interface IAuditLogRepository
    {
        #region Phương thức ghi log cơ bản

        /// <summary>
        /// Ghi một bản ghi audit log mới vào cơ sở dữ liệu
        /// Sử dụng để theo dõi các hoạt động của người dùng trong hệ thống
        /// </summary>
        /// <param name="auditLog">Đối tượng AuditLog chứa thông tin hoạt động cần ghi lại</param>
        /// <returns>Task đại diện cho quá trình ghi log bất đồng bộ</returns>
        Task AddAsync(AuditLog auditLog);

        /// <summary>
        /// Ghi một bản ghi audit log với thông tin chi tiết
        /// Phương thức tiện ích để tạo và ghi log nhanh chóng
        /// </summary>
        /// <param name="userId">ID của người dùng thực hiện hoạt động (null nếu là system action)</param>
        /// <param name="action">Mô tả hành động được thực hiện (VD: "Login", "Create VocaSet", "Delete Folder")</param>
        /// <param name="details">Chi tiết bổ sung về hoạt động (có thể là JSON hoặc text)</param>
        /// <returns>Task đại diện cho quá trình ghi log bất đồng bộ</returns>
        Task LogAsync(string? userId, string action, string? details = null);

        /// <summary>
        /// Ghi nhiều bản ghi audit log cùng lúc
        /// Sử dụng cho các thao tác batch hoặc khi cần ghi nhiều log trong một transaction
        /// </summary>
        /// <param name="auditLogs">Danh sách các AuditLog cần ghi</param>
        /// <returns>Task đại diện cho quá trình ghi log bất đồng bộ</returns>
        Task AddRangeAsync(IEnumerable<AuditLog> auditLogs);

        #endregion

        #region Phương thức truy vấn cơ bản

        /// <summary>
        /// Lấy một bản ghi audit log theo ID
        /// </summary>
        /// <param name="id">ID của bản ghi audit log</param>
        /// <returns>Đối tượng AuditLog nếu tìm thấy, null nếu không tìm thấy</returns>
        Task<AuditLog?> GetByIdAsync(int id);

        /// <summary>
        /// Lấy tất cả các bản ghi audit log
        /// Chú ý: Chỉ sử dụng cho mục đích testing hoặc khi chắc chắn dữ liệu ít
        /// </summary>
        /// <returns>Danh sách tất cả AuditLog trong hệ thống</returns>
        Task<IEnumerable<AuditLog>> GetAllAsync();

        #endregion

        #region Phương thức truy vấn theo điều kiện

        /// <summary>
        /// Lấy tất cả các bản ghi audit log của một người dùng cụ thể
        /// Sử dụng để xem lịch sử hoạt động của một user
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Danh sách AuditLog của người dùng được sắp xếp theo thời gian mới nhất</returns>
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Lấy các bản ghi audit log theo loại hành động
        /// Sử dụng để phân tích các hoạt động cụ thể (VD: tất cả các lần đăng nhập)
        /// </summary>
        /// <param name="action">Tên hành động cần tìm kiếm</param>
        /// <returns>Danh sách AuditLog có cùng loại hành động</returns>
        Task<IEnumerable<AuditLog>> GetByActionAsync(string action);

        /// <summary>
        /// Lấy các bản ghi audit log trong khoảng thời gian nhất định
        /// Sử dụng để tạo báo cáo hoạt động theo thời gian
        /// </summary>
        /// <param name="fromDate">Thời gian bắt đầu</param>
        /// <param name="toDate">Thời gian kết thúc</param>
        /// <returns>Danh sách AuditLog trong khoảng thời gian chỉ định</returns>
        Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Lấy các bản ghi audit log của một người dùng trong khoảng thời gian nhất định
        /// Kết hợp filter theo user và thời gian
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="fromDate">Thời gian bắt đầu</param>
        /// <param name="toDate">Thời gian kết thúc</param>
        /// <returns>Danh sách AuditLog của user trong khoảng thời gian</returns>
        Task<IEnumerable<AuditLog>> GetByUserAndDateRangeAsync(string userId, DateTime fromDate, DateTime toDate);

        #endregion

        #region Phương thức truy vấn với phân trang

        /// <summary>
        /// Lấy danh sách audit log có phân trang
        /// Sử dụng cho việc hiển thị dữ liệu trên giao diện với khối lượng lớn
        /// </summary>
        /// <param name="pageNumber">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số bản ghi trên mỗi trang</param>
        /// <returns>Danh sách AuditLog được phân trang</returns>
        Task<IEnumerable<AuditLog>> GetPagedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Lấy danh sách audit log của một user có phân trang
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="pageNumber">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số bản ghi trên mỗi trang</param>
        /// <returns>Danh sách AuditLog của user được phân trang</returns>
        Task<IEnumerable<AuditLog>> GetPagedByUserAsync(string userId, int pageNumber, int pageSize);

        #endregion

        #region Phương thức tìm kiếm và thống kê

        /// <summary>
        /// Tìm kiếm audit log theo từ khóa trong Action hoặc Details
        /// Sử dụng cho chức năng tìm kiếm trong admin panel
        /// </summary>
        /// <param name="keyword">Từ khóa cần tìm kiếm</param>
        /// <returns>Danh sách AuditLog chứa từ khóa</returns>
        Task<IEnumerable<AuditLog>> SearchAsync(string keyword);

        /// <summary>
        /// Đếm tổng số bản ghi audit log
        /// Sử dụng cho việc hiển thị thống kê tổng quan
        /// </summary>
        /// <returns>Tổng số bản ghi trong bảng AuditLog</returns>
        Task<int> GetTotalCountAsync();

        /// <summary>
        /// Đếm số bản ghi audit log của một người dùng
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Tổng số hoạt động của người dùng</returns>
        Task<int> GetCountByUserAsync(string userId);

        /// <summary>
        /// Lấy các hoạt động mới nhất của hệ thống
        /// Sử dụng cho dashboard admin để theo dõi hoạt động real-time
        /// </summary>
        /// <param name="limit">Số lượng bản ghi mới nhất cần lấy</param>
        /// <returns>Danh sách AuditLog mới nhất</returns>
        Task<IEnumerable<AuditLog>> GetRecentActivitiesAsync(int limit = 10);

        /// <summary>
        /// Lấy thống kê số lượng hoạt động theo ngày
        /// Sử dụng cho việc tạo biểu đồ hoạt động hàng ngày
        /// </summary>
        /// <param name="days">Số ngày gần đây cần thống kê</param>
        /// <returns>Dictionary với key là ngày, value là số lượng hoạt động</returns>
        Task<Dictionary<DateTime, int>> GetDailyActivityStatsAsync(int days = 7);

        /// <summary>
        /// Lấy thống kê các loại hoạt động phổ biến
        /// Sử dụng để phân tích hành vi người dùng
        /// </summary>
        /// <param name="limit">Số lượng loại hoạt động top cần lấy</param>
        /// <returns>Dictionary với key là tên action, value là số lần xuất hiện</returns>
        Task<Dictionary<string, int>> GetTopActionsAsync(int limit = 10);

        #endregion

        #region Phương thức quản lý dữ liệu

        /// <summary>
        /// Xóa các bản ghi audit log cũ hơn số ngày chỉ định
        /// Sử dụng cho việc dọn dẹp dữ liệu định kỳ để tối ưu hiệu suất
        /// </summary>
        /// <param name="days">Số ngày, các bản ghi cũ hơn sẽ bị xóa</param>
        /// <returns>Số lượng bản ghi đã được xóa</returns>
        Task<int> DeleteOldRecordsAsync(int days);

        /// <summary>
        /// Kiểm tra xem có bản ghi nào tồn tại không
        /// Sử dụng cho việc kiểm tra trước khi thực hiện các thao tác khác
        /// </summary>
        /// <returns>True nếu có ít nhất một bản ghi, False nếu bảng rỗng</returns>
        Task<bool> AnyAsync();

        /// <summary>
        /// Lưu các thay đổi vào cơ sở dữ liệu
        /// Phương thức này sẽ commit tất cả các thay đổi đang pending
        /// </summary>
        /// <returns>Số lượng bản ghi đã được lưu</returns>
        Task<int> SaveChangesAsync();

        #endregion
    }
}