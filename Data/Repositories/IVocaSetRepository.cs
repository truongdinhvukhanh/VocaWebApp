using System.Linq.Expressions;
using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    /// <summary>
    /// Interface định nghĩa các phương thức để quản lý VocaSet trong hệ thống
    /// Hỗ trợ CRUD operations, tìm kiếm, phân trang và các tính năng đặc biệt
    /// </summary>
    public interface IVocaSetRepository
    {
        #region CRUD Operations

        /// <summary>
        /// Lấy VocaSet theo ID
        /// </summary>
        /// <param name="id">ID của VocaSet</param>
        /// <returns>VocaSet hoặc null nếu không tìm thấy</returns>
        Task<VocaSet?> GetByIdAsync(int id);

        /// <summary>
        /// Lấy VocaSet theo ID bao gồm các navigation properties
        /// </summary>
        /// <param name="id">ID của VocaSet</param>
        /// <param name="includeProperties">Các navigation properties cần include</param>
        /// <returns>VocaSet với đầy đủ thông tin hoặc null</returns>
        Task<VocaSet?> GetByIdWithIncludesAsync(int id, params string[] includeProperties);

        /// <summary>
        /// Lấy tất cả VocaSet (không bao gồm deleted)
        /// </summary>
        /// <returns>Danh sách VocaSet</returns>
        Task<IEnumerable<VocaSet>> GetAllAsync();

        /// <summary>
        /// Thêm VocaSet mới
        /// </summary>
        /// <param name="vocaSet">VocaSet cần thêm</param>
        /// <returns>VocaSet đã được thêm</returns>
        Task<VocaSet> AddAsync(VocaSet vocaSet);

        /// <summary>
        /// Cập nhật VocaSet
        /// </summary>
        /// <param name="vocaSet">VocaSet cần cập nhật</param>
        /// <returns>VocaSet đã được cập nhật</returns>
        Task<VocaSet> UpdateAsync(VocaSet vocaSet);

        /// <summary>
        /// Xóa mềm VocaSet (đặt IsDeleted = true)
        /// </summary>
        /// <param name="id">ID của VocaSet cần xóa</param>
        /// <returns>True nếu xóa thành công</returns>
        Task<bool> SoftDeleteAsync(int id);

        /// <summary>
        /// Xóa cứng VocaSet khỏi database
        /// </summary>
        /// <param name="id">ID của VocaSet cần xóa</param>
        /// <returns>True nếu xóa thành công</returns>
        Task<bool> HardDeleteAsync(int id);

        #endregion

        #region User-specific Operations

        /// <summary>
        /// Lấy tất cả VocaSet của user cụ thể
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="includeDeleted">Có bao gồm VocaSet đã xóa không</param>
        /// <returns>Danh sách VocaSet của user</returns>
        Task<IEnumerable<VocaSet>> GetByUserIdAsync(string userId, bool includeDeleted = false);

        /// <summary>
        /// Lấy VocaSet theo user và folder
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="folderId">ID của folder (null để lấy VocaSet không có folder)</param>
        /// <returns>Danh sách VocaSet trong folder</returns>
        Task<IEnumerable<VocaSet>> GetByUserAndFolderAsync(string userId, int? folderId);

        /// <summary>
        /// Lấy VocaSet truy cập gần đây của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="limit">Số lượng giới hạn</param>
        /// <returns>Danh sách VocaSet truy cập gần đây</returns>
        Task<IEnumerable<VocaSet>> GetRecentlyAccessedAsync(string userId, int limit = 10);

        #endregion

        #region Search and Filter Operations

        /// <summary>
        /// Tìm kiếm VocaSet theo từ khóa
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="userId">ID của user (null để tìm trong public VocaSet)</param>
        /// <param name="searchInPublic">Có tìm trong VocaSet public không</param>
        /// <returns>Danh sách VocaSet phù hợp</returns>
        Task<IEnumerable<VocaSet>> SearchAsync(string keyword, string? userId = null, bool searchInPublic = true);

        /// <summary>
        /// Lọc VocaSet theo điều kiện
        /// </summary>
        /// <param name="predicate">Điều kiện lọc</param>
        /// <returns>Danh sách VocaSet phù hợp</returns>
        Task<IEnumerable<VocaSet>> FindAsync(Expression<Func<VocaSet, bool>> predicate);

        /// <summary>
        /// Lấy VocaSet public có thể xem
        /// </summary>
        /// <param name="excludeUserId">Loại trừ VocaSet của user này</param>
        /// <returns>Danh sách VocaSet public</returns>
        Task<IEnumerable<VocaSet>> GetPublicViewableAsync(string? excludeUserId = null);

        /// <summary>
        /// Lấy VocaSet public có thể sao chép
        /// </summary>
        /// <param name="excludeUserId">Loại trừ VocaSet của user này</param>
        /// <returns>Danh sách VocaSet có thể sao chép</returns>
        Task<IEnumerable<VocaSet>> GetPublicCopyableAsync(string? excludeUserId = null);

        #endregion

        #region Status and Visibility Management

        /// <summary>
        /// Cập nhật trạng thái VocaSet
        /// </summary>
        /// <param name="id">ID của VocaSet</param>
        /// <param name="status">Trạng thái mới (private, public-view, public-copy)</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> UpdateStatusAsync(int id, string status);

        /// <summary>
        /// Ẩn/hiện VocaSet (admin function)
        /// </summary>
        /// <param name="id">ID của VocaSet</param>
        /// <param name="isHidden">True để ẩn, false để hiện</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> UpdateVisibilityAsync(int id, bool isHidden);

        /// <summary>
        /// Khôi phục VocaSet đã xóa mềm
        /// </summary>
        /// <param name="id">ID của VocaSet</param>
        /// <returns>True nếu khôi phục thành công</returns>
        Task<bool> RestoreAsync(int id);

        #endregion

        #region Statistics and Analytics

        /// <summary>
        /// Tăng số lượt xem VocaSet
        /// </summary>
        /// <param name="id">ID của VocaSet</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> IncrementViewCountAsync(int id);

        /// <summary>
        /// Cập nhật thời gian truy cập cuối
        /// </summary>
        /// <param name="id">ID của VocaSet</param>
        /// <param name="accessTime">Thời gian truy cập (null để dùng thời gian hiện tại)</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> UpdateLastAccessedAsync(int id, DateTime? accessTime = null);

        /// <summary>
        /// Lấy thống kê VocaSet của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Thống kê VocaSet</returns>
        Task<VocaSetStatistics> GetUserStatisticsAsync(string userId);

        /// <summary>
        /// Lấy VocaSet phổ biến nhất
        /// </summary>
        /// <param name="limit">Số lượng giới hạn</param>
        /// <param name="timeFrame">Khoảng thời gian (ngày)</param>
        /// <returns>Danh sách VocaSet phổ biến</returns>
        Task<IEnumerable<VocaSet>> GetMostPopularAsync(int limit = 10, int? timeFrame = null);

        #endregion

        #region Copy Operations

        /// <summary>
        /// Sao chép VocaSet public
        /// </summary>
        /// <param name="originalSetId">ID của VocaSet gốc</param>
        /// <param name="userId">ID của user thực hiện sao chép</param>
        /// <param name="newName">Tên mới cho VocaSet (null để giữ tên gốc)</param>
        /// <param name="folderId">ID folder đích (null để không đặt vào folder)</param>
        /// <returns>VocaSet đã được sao chép</returns>
        Task<VocaSet?> CopyVocaSetAsync(int originalSetId, string userId, string? newName = null, int? folderId = null);

        /// <summary>
        /// Kiểm tra VocaSet có thể sao chép không
        /// </summary>
        /// <param name="id">ID của VocaSet</param>
        /// <param name="userId">ID của user muốn sao chép</param>
        /// <returns>True nếu có thể sao chép</returns>
        Task<bool> CanCopyAsync(int id, string userId);

        #endregion

        #region Pagination and Sorting

        /// <summary>
        /// Lấy VocaSet với phân trang
        /// </summary>
        /// <param name="pageNumber">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <param name="userId">ID của user (null để lấy tất cả public)</param>
        /// <param name="sortBy">Sắp xếp theo (name, created, accessed, viewcount)</param>
        /// <param name="sortDescending">Sắp xếp giảm dần</param>
        /// <returns>Kết quả phân trang</returns>
        Task<PagedResult<VocaSet>> GetPagedAsync(int pageNumber, int pageSize, string? userId = null,
            string sortBy = "created", bool sortDescending = true);

        /// <summary>
        /// Đếm tổng số VocaSet theo điều kiện
        /// </summary>
        /// <param name="predicate">Điều kiện đếm</param>
        /// <returns>Tổng số VocaSet</returns>
        Task<int> CountAsync(Expression<Func<VocaSet, bool>>? predicate = null);

        #endregion

        #region Validation and Business Rules

        /// <summary>
        /// Kiểm tra user có quyền truy cập VocaSet không
        /// </summary>
        /// <param name="vocaSetId">ID của VocaSet</param>
        /// <param name="userId">ID của user</param>
        /// <returns>True nếu có quyền truy cập</returns>
        Task<bool> HasAccessAsync(int vocaSetId, string userId);

        /// <summary>
        /// Kiểm tra user có quyền chỉnh sửa VocaSet không
        /// </summary>
        /// <param name="vocaSetId">ID của VocaSet</param>
        /// <param name="userId">ID của user</param>
        /// <returns>True nếu có quyền chỉnh sửa</returns>
        Task<bool> HasEditPermissionAsync(int vocaSetId, string userId);

        /// <summary>
        /// Kiểm tra tên VocaSet có trùng không
        /// </summary>
        /// <param name="name">Tên VocaSet</param>
        /// <param name="userId">ID của user</param>
        /// <param name="excludeId">Loại trừ VocaSet này khỏi kiểm tra</param>
        /// <returns>True nếu tên đã tồn tại</returns>
        Task<bool> IsNameExistsAsync(string name, string userId, int? excludeId = null);

        #endregion
    }

    #region Support Classes

    /// <summary>
    /// Class chứa thống kê VocaSet của user
    /// </summary>
    public class VocaSetStatistics
    {
        public int TotalVocaSets { get; set; }
        public int PublicVocaSets { get; set; }
        public int PrivateVocaSets { get; set; }
        public int TotalWords { get; set; }
        public int TotalViews { get; set; }
        public DateTime? LastCreated { get; set; }
        public DateTime? LastAccessed { get; set; }
    }

    /// <summary>
    /// Class chứa kết quả phân trang
    /// </summary>
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    #endregion

}