using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    /// <summary>
    /// Interface định nghĩa các phương thức để quản lý VocaItem trong cơ sở dữ liệu
    /// Hỗ trợ các tính năng học từ vựng, flashcard, theo dõi tiến độ
    /// </summary>
    public interface IVocaItemRepository
    {
        #region Các phương thức CRUD cơ bản

        /// <summary>
        /// Lấy tất cả VocaItem trong hệ thống
        /// </summary>
        Task<IEnumerable<VocaItem>> GetAllAsync();

        /// <summary>
        /// Lấy VocaItem theo ID
        /// </summary>
        Task<VocaItem?> GetByIdAsync(int id);

        /// <summary>
        /// Thêm VocaItem mới vào cơ sở dữ liệu
        /// </summary>
        Task<VocaItem> AddAsync(VocaItem vocaItem);

        /// <summary>
        /// Cập nhật thông tin VocaItem
        /// </summary>
        Task<VocaItem> UpdateAsync(VocaItem vocaItem);

        /// <summary>
        /// Xóa VocaItem khỏi cơ sở dữ liệu
        /// </summary>
        Task<bool> DeleteAsync(int id);

        #endregion

        #region Các phương thức lấy dữ liệu theo VocaSet

        /// <summary>
        /// Lấy tất cả VocaItem thuộc một VocaSet cụ thể
        /// </summary>
        Task<IEnumerable<VocaItem>> GetByVocaSetIdAsync(int vocaSetId);

        /// <summary>
        /// Lấy VocaItem với phân trang theo VocaSet
        /// </summary>
        Task<IEnumerable<VocaItem>> GetByVocaSetIdWithPaginationAsync(int vocaSetId, int pageNumber, int pageSize);

        /// <summary>
        /// Đếm tổng số VocaItem trong một VocaSet
        /// </summary>
        Task<int> GetCountByVocaSetIdAsync(int vocaSetId);

        #endregion

        #region Các phương thức cho tính năng học tập và flashcard

        /// <summary>
        /// Lấy VocaItem theo trạng thái học tập
        /// </summary>
        Task<IEnumerable<VocaItem>> GetByStatusAsync(int vocaSetId, string status);

        /// <summary>
        /// Lấy VocaItem chưa học để luyện tập flashcard
        /// </summary>
        Task<IEnumerable<VocaItem>> GetUnlearnedForFlashcardAsync(int vocaSetId, int limit);

        /// <summary>
        /// Lấy VocaItem ngẫu nhiên để luyện tập flashcard
        /// </summary>
        Task<IEnumerable<VocaItem>> GetRandomForFlashcardAsync(int vocaSetId, int count, bool includeOnlyUnlearned = false);

        /// <summary>
        /// Cập nhật trạng thái học tập của VocaItem
        /// </summary>
        Task<bool> UpdateLearningStatusAsync(int vocaItemId, string newStatus);

        #endregion

        #region Các phương thức tìm kiếm

        /// <summary>
        /// Tìm kiếm VocaItem theo từ khóa trong một VocaSet
        /// </summary>
        Task<IEnumerable<VocaItem>> SearchInVocaSetAsync(int vocaSetId, string keyword);

        /// <summary>
        /// Tìm kiếm VocaItem theo từ vựng chính xác
        /// </summary>
        Task<VocaItem?> FindByWordAsync(int vocaSetId, string word);

        #endregion

        #region Các phương thức thống kê và báo cáo

        /// <summary>
        /// Lấy thống kê học tập của một VocaSet
        /// </summary>
        Task<Dictionary<string, int>> GetLearningStatisticsAsync(int vocaSetId);

        /// <summary>
        /// Lấy tiến độ học tập theo phần trăm
        /// </summary>
        Task<double> GetLearningProgressPercentageAsync(int vocaSetId);

        #endregion

        #region Các phương thức hỗ trợ bulk operations

        /// <summary>
        /// Thêm nhiều VocaItem cùng lúc
        /// </summary>
        Task<IEnumerable<VocaItem>> AddRangeAsync(IEnumerable<VocaItem> vocaItems);

        /// <summary>
        /// Xóa tất cả VocaItem trong một VocaSet
        /// </summary>
        Task<int> DeleteByVocaSetIdAsync(int vocaSetId);

        #endregion

        #region Các phương thức với Include relationships

        /// <summary>
        /// Lấy VocaItem với thông tin VocaSet
        /// </summary>
        Task<VocaItem?> GetWithVocaSetAsync(int id);

        /// <summary>
        /// Lấy VocaItem với lịch sử học tập
        /// </summary>
        Task<VocaItem?> GetWithHistoriesAsync(int id);

        #endregion
    }
}
