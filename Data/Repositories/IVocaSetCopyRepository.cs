using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    /// <summary>
    /// Interface định nghĩa các phương thức cần thiết để quản lý việc sao chép bộ từ vựng
    /// Hỗ trợ các tính năng copy bộ từ vựng public và quản lý lịch sử copy
    /// </summary>
    public interface IVocaSetCopyRepository
    {
        /// <summary>
        /// Tạo một bản sao mới của bộ từ vựng
        /// Sử dụng khi người dùng copy một bộ từ vựng public
        /// </summary>
        /// <param name="originalSetId">ID của bộ từ vựng gốc cần copy</param>
        /// <param name="copiedByUserId">ID của người dùng thực hiện copy</param>
        /// <returns>VocaSetCopy entity đã được tạo</returns>
        Task<VocaSetCopy> CreateCopyAsync(int originalSetId, string copiedByUserId);

        /// <summary>
        /// Lấy thông tin chi tiết của một bản copy theo ID
        /// Bao gồm thông tin về bộ từ vựng gốc và người copy
        /// </summary>
        /// <param name="id">ID của bản copy</param>
        /// <returns>VocaSetCopy entity hoặc null nếu không tìm thấy</returns>
        Task<VocaSetCopy?> GetByIdAsync(int id);

        /// <summary>
        /// Lấy danh sách tất cả các bản copy của một bộ từ vựng cụ thể
        /// Sử dụng để xem ai đã copy bộ từ vựng này (cho admin hoặc chủ sở hữu)
        /// </summary>
        /// <param name="originalSetId">ID của bộ từ vựng gốc</param>
        /// <returns>Danh sách các bản copy</returns>
        Task<IEnumerable<VocaSetCopy>> GetCopiesByOriginalSetIdAsync(int originalSetId);

        /// <summary>
        /// Lấy danh sách tất cả các bộ từ vựng mà một người dùng đã copy
        /// Sử dụng trong trang profile hoặc dashboard của người dùng
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Danh sách các bản copy của người dùng</returns>
        Task<IEnumerable<VocaSetCopy>> GetCopiesByUserIdAsync(string userId);

        /// <summary>
        /// Kiểm tra xem một người dùng đã copy một bộ từ vựng cụ thể chưa
        /// Tránh việc copy trùng lặp cùng một bộ từ vựng
        /// </summary>
        /// <param name="originalSetId">ID của bộ từ vựng gốc</param>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>True nếu đã copy, False nếu chưa copy</returns>
        Task<bool> HasUserCopiedSetAsync(int originalSetId, string userId);

        /// <summary>
        /// Lấy danh sách các bản copy mới nhất trong hệ thống
        /// Sử dụng cho dashboard admin hoặc thống kê hoạt động
        /// </summary>
        /// <param name="count">Số lượng bản copy muốn lấy</param>
        /// <returns>Danh sách các bản copy mới nhất</returns>
        Task<IEnumerable<VocaSetCopy>> GetRecentCopiesAsync(int count = 10);

        /// <summary>
        /// Đếm số lượng bản copy của một bộ từ vựng cụ thể
        /// Sử dụng để hiển thị độ phổ biến của bộ từ vựng
        /// </summary>
        /// <param name="originalSetId">ID của bộ từ vựng gốc</param>
        /// <returns>Số lượng bản copy</returns>
        Task<int> GetCopyCountByOriginalSetIdAsync(int originalSetId);

        /// <summary>
        /// Lấy danh sách các bộ từ vựng được copy nhiều nhất
        /// Sử dụng cho trang thống kê hoặc trang khám phá hot trends
        /// </summary>
        /// <param name="count">Số lượng bộ từ vựng muốn lấy</param>
        /// <returns>Danh sách các bộ từ vựng phổ biến cùng số lần copy</returns>
        Task<IEnumerable<object>> GetMostCopiedSetsAsync(int count = 10);

        /// <summary>
        /// Xóa một bản copy khỏi hệ thống
        /// Sử dụng khi cần dọn dẹp dữ liệu hoặc người dùng yêu cầu xóa
        /// </summary>
        /// <param name="id">ID của bản copy cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Lấy thống kê copy theo khoảng thời gian
        /// Sử dụng cho báo cáo admin về hoạt động copy trong hệ thống
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns>Số lượng copy trong khoảng thời gian</returns>
        Task<int> GetCopyCountByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}