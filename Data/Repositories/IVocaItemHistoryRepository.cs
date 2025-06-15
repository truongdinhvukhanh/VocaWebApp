using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    /// <summary>
    /// Interface định nghĩa các phương thức repository cho VocaItemHistory
    /// Quản lý lịch sử học tập của người dùng với từng từ vựng
    /// </summary>
    public interface IVocaItemHistoryRepository
    {
        #region Phương thức CRUD cơ bản

        /// <summary>
        /// Thêm mới một bản ghi lịch sử học tập
        /// </summary>
        /// <param name="history">Đối tượng VocaItemHistory cần thêm</param>
        /// <returns>Task<VocaItemHistory> - Đối tượng vừa được thêm</returns>
        Task<VocaItemHistory> AddAsync(VocaItemHistory history);

        /// <summary>
        /// Cập nhật thông tin lịch sử học tập
        /// </summary>
        /// <param name="history">Đối tượng VocaItemHistory cần cập nhật</param>
        /// <returns>Task<VocaItemHistory> - Đối tượng sau khi cập nhật</returns>
        Task<VocaItemHistory> UpdateAsync(VocaItemHistory history);

        /// <summary>
        /// Xóa một bản ghi lịch sử học tập theo ID
        /// </summary>
        /// <param name="id">ID của bản ghi cần xóa</param>
        /// <returns>Task<bool> - True nếu xóa thành công</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Lấy thông tin lịch sử học tập theo ID
        /// </summary>
        /// <param name="id">ID của bản ghi lịch sử</param>
        /// <returns>Task<VocaItemHistory> - Đối tượng VocaItemHistory hoặc null</returns>
        Task<VocaItemHistory?> GetByIdAsync(int id);

        #endregion

        #region Phương thức lấy lịch sử theo người dùng

        /// <summary>
        /// Lấy toàn bộ lịch sử học tập của một người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Task<IEnumerable<VocaItemHistory>> - Danh sách lịch sử học tập</returns>
        Task<IEnumerable<VocaItemHistory>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Lấy lịch sử học tập gần đây của người dùng (có phân trang)
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="pageNumber">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số bản ghi mỗi trang</param>
        /// <returns>Task<IEnumerable<VocaItemHistory>> - Danh sách lịch sử học tập đã phân trang</returns>
        Task<IEnumerable<VocaItemHistory>> GetRecentByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 20);

        /// <summary>
        /// Lấy lịch sử học tập của người dùng trong khoảng thời gian
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="fromDate">Ngày bắt đầu</param>
        /// <param name="toDate">Ngày kết thúc</param>
        /// <returns>Task<IEnumerable<VocaItemHistory>> - Danh sách lịch sử trong khoảng thời gian</returns>
        Task<IEnumerable<VocaItemHistory>> GetByUserIdAndDateRangeAsync(string userId, DateTime fromDate, DateTime toDate);

        #endregion

        #region Phương thức lấy lịch sử theo từ vựng

        /// <summary>
        /// Lấy lịch sử học tập của một từ vựng cụ thể
        /// </summary>
        /// <param name="vocaItemId">ID từ vựng</param>
        /// <returns>Task<IEnumerable<VocaItemHistory>> - Danh sách lịch sử học từ vựng</returns>
        Task<IEnumerable<VocaItemHistory>> GetByVocaItemIdAsync(int vocaItemId);

        /// <summary>
        /// Lấy lịch sử học tập của người dùng với một từ vựng cụ thể
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaItemId">ID từ vựng</param>
        /// <returns>Task<IEnumerable<VocaItemHistory>> - Danh sách lịch sử học từ vựng của người dùng</returns>
        Task<IEnumerable<VocaItemHistory>> GetByUserIdAndVocaItemIdAsync(string userId, int vocaItemId);

        /// <summary>
        /// Lấy lịch sử học tập mới nhất của người dùng với một từ vựng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaItemId">ID từ vựng</param>
        /// <returns>Task<VocaItemHistory> - Bản ghi lịch sử mới nhất hoặc null</returns>
        Task<VocaItemHistory?> GetLatestByUserIdAndVocaItemIdAsync(string userId, int vocaItemId);

        #endregion

        #region Phương thức cập nhật trạng thái học tập

        /// <summary>
        /// Cập nhật trạng thái học tập của một từ vựng
        /// Tự động tạo bản ghi lịch sử mới với trạng thái và thời gian hiện tại
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaItemId">ID từ vựng</param>
        /// <param name="status">Trạng thái mới (learned, notlearned, reviewing, etc.)</param>
        /// <returns>Task<VocaItemHistory> - Bản ghi lịch sử vừa tạo</returns>
        Task<VocaItemHistory> UpdateLearningStatusAsync(string userId, int vocaItemId, string status);

        /// <summary>
        /// Đánh dấu nhiều từ vựng đã học trong một lần
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaItemIds">Danh sách ID từ vựng</param>
        /// <param name="status">Trạng thái chung</param>
        /// <returns>Task<IEnumerable<VocaItemHistory>> - Danh sách bản ghi lịch sử vừa tạo</returns>
        Task<IEnumerable<VocaItemHistory>> UpdateMultipleLearningStatusAsync(string userId, IEnumerable<int> vocaItemIds, string status);

        #endregion

        #region Phương thức thống kê và báo cáo

        /// <summary>
        /// Đếm tổng số từ đã học của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Task<int> - Số lượng từ đã học</returns>
        Task<int> CountLearnedWordsAsync(string userId);

        /// <summary>
        /// Đếm số từ học trong ngày hôm nay
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Task<int> - Số từ học hôm nay</returns>
        Task<int> CountTodayLearnedWordsAsync(string userId);

        /// <summary>
        /// Lấy thống kê học tập của người dùng trong một bộ từ vựng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaSetId">ID bộ từ vựng</param>
        /// <returns>Task<Dictionary<string, int>> - Dictionary chứa thống kê theo trạng thái</returns>
        Task<Dictionary<string, int>> GetLearningStatisticsByVocaSetAsync(string userId, int vocaSetId);

        /// <summary>
        /// Lấy thống kê tổng quan học tập của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Task<Dictionary<string, int>> - Dictionary chứa các thống kê tổng quan</returns>
        Task<Dictionary<string, int>> GetOverallLearningStatisticsAsync(string userId);

        /// <summary>
        /// Lấy lịch sử học tập theo tuần/tháng để hiển thị biểu đồ tiến độ
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="days">Số ngày trước đó cần lấy dữ liệu</param>
        /// <returns>Task<Dictionary<DateTime, int>> - Dictionary với key là ngày và value là số từ học</returns>
        Task<Dictionary<DateTime, int>> GetLearningProgressChartDataAsync(string userId, int days = 30);

        #endregion

        #region Phương thức hỗ trợ học tập

        /// <summary>
        /// Lấy danh sách từ vựng cần ôn tập của người dùng
        /// (Dựa trên lịch sử học và thời gian ôn tập)
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="reviewIntervalDays">Khoảng cách ngày ôn tập (mặc định 7 ngày)</param>
        /// <returns>Task<IEnumerable<int>> - Danh sách ID từ vựng cần ôn tập</returns>
        Task<IEnumerable<int>> GetVocaItemsNeedReviewAsync(string userId, int reviewIntervalDays = 7);

        /// <summary>
        /// Kiểm tra xem một từ vựng đã được học hay chưa
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaItemId">ID từ vựng</param>
        /// <returns>Task<bool> - True nếu đã học</returns>
        Task<bool> IsVocaItemLearnedAsync(string userId, int vocaItemId);

        /// <summary>
        /// Lấy trạng thái học tập hiện tại của một từ vựng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaItemId">ID từ vựng</param>
        /// <returns>Task<string> - Trạng thái hiện tại hoặc "notlearned" nếu chưa có lịch sử</returns>
        Task<string> GetCurrentLearningStatusAsync(string userId, int vocaItemId);

        #endregion
    }
}