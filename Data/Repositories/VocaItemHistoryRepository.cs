using Microsoft.EntityFrameworkCore;
using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    /// <summary>
    /// Repository implementation cho VocaItemHistory
    /// Triển khai các phương thức quản lý lịch sử học tập của người dùng
    /// </summary>
    public class VocaItemHistoryRepository : IVocaItemHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor khởi tạo repository với DbContext
        /// </summary>
        /// <param name="context">ApplicationDbContext instance</param>
        public VocaItemHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Phương thức CRUD cơ bản

        /// <summary>
        /// Thêm mới một bản ghi lịch sử học tập
        /// </summary>
        /// <param name="history">Đối tượng VocaItemHistory cần thêm</param>
        /// <returns>Task<VocaItemHistory> - Đối tượng vừa được thêm</returns>
        public async Task<VocaItemHistory> AddAsync(VocaItemHistory history)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));

            // Đặt thời gian ReviewedAt nếu chưa được set
            if (history.ReviewedAt == default)
                history.ReviewedAt = DateTime.UtcNow;

            _context.VocaItemHistories.Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

        /// <summary>
        /// Cập nhật thông tin lịch sử học tập
        /// </summary>
        /// <param name="history">Đối tượng VocaItemHistory cần cập nhật</param>
        /// <returns>Task<VocaItemHistory> - Đối tượng sau khi cập nhật</returns>
        public async Task<VocaItemHistory> UpdateAsync(VocaItemHistory history)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));

            _context.VocaItemHistories.Update(history);
            await _context.SaveChangesAsync();
            return history;
        }

        /// <summary>
        /// Xóa một bản ghi lịch sử học tập theo ID
        /// </summary>
        /// <param name="id">ID của bản ghi cần xóa</param>
        /// <returns>Task<bool> - True nếu xóa thành công</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var history = await _context.VocaItemHistories.FindAsync(id);
            if (history == null)
                return false;

            _context.VocaItemHistories.Remove(history);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Lấy thông tin lịch sử học tập theo ID
        /// </summary>
        /// <param name="id">ID của bản ghi lịch sử</param>
        /// <returns>Task<VocaItemHistory> - Đối tượng VocaItemHistory hoặc null</returns>
        public async Task<VocaItemHistory?> GetByIdAsync(int id)
        {
            return await _context.VocaItemHistories
                .Include(h => h.User)
                .Include(h => h.VocaItem)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        #endregion

        #region Phương thức lấy lịch sử theo người dùng

        /// <summary>
        /// Lấy toàn bộ lịch sử học tập của một người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Task<IEnumerable<VocaItemHistory>> - Danh sách lịch sử học tập</returns>
        public async Task<IEnumerable<VocaItemHistory>> GetByUserIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            return await _context.VocaItemHistories
                .Where(h => h.UserId == userId)
                .Include(h => h.VocaItem)
                    .ThenInclude(v => v.VocaSet)
                .OrderByDescending(h => h.ReviewedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy lịch sử học tập gần đây của người dùng (có phân trang)
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="pageNumber">Số trang (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số bản ghi mỗi trang</param>
        /// <returns>Task<IEnumerable<VocaItemHistory>> - Danh sách lịch sử học tập đã phân trang</returns>
        public async Task<IEnumerable<VocaItemHistory>> GetRecentByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 20)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;

            return await _context.VocaItemHistories
                .Where(h => h.UserId == userId)
                .Include(h => h.VocaItem)
                    .ThenInclude(v => v.VocaSet)
                .OrderByDescending(h => h.ReviewedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy lịch sử học tập của người dùng trong khoảng thời gian
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="fromDate">Ngày bắt đầu</param>
        /// <param name="toDate">Ngày kết thúc</param>
        /// <returns>Task<IEnumerable<VocaItemHistory>> - Danh sách lịch sử trong khoảng thời gian</returns>
        public async Task<IEnumerable<VocaItemHistory>> GetByUserIdAndDateRangeAsync(string userId, DateTime fromDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            return await _context.VocaItemHistories
                .Where(h => h.UserId == userId &&
                           h.ReviewedAt >= fromDate &&
                           h.ReviewedAt <= toDate)
                .Include(h => h.VocaItem)
                    .ThenInclude(v => v.VocaSet)
                .OrderByDescending(h => h.ReviewedAt)
                .ToListAsync();
        }

        #endregion

        #region Phương thức lấy lịch sử theo từ vựng

        /// <summary>
        /// Lấy lịch sử học tập của một từ vựng cụ thể
        /// </summary>
        /// <param name="vocaItemId">ID từ vựng</param>
        /// <returns>Task<IEnumerable<VocaItemHistory>> - Danh sách lịch sử học từ vựng</returns>
        public async Task<IEnumerable<VocaItemHistory>> GetByVocaItemIdAsync(int vocaItemId)
        {
            return await _context.VocaItemHistories
                .Where(h => h.VocaItemId == vocaItemId)
                .Include(h => h.User)
                .OrderByDescending(h => h.ReviewedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy lịch sử học tập của người dùng với một từ vựng cụ thể
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaItemId">ID từ vựng</param>
        /// <returns>Task<IEnumerable<VocaItemHistory>> - Danh sách lịch sử học từ vựng của người dùng</returns>
        public async Task<IEnumerable<VocaItemHistory>> GetByUserIdAndVocaItemIdAsync(string userId, int vocaItemId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            return await _context.VocaItemHistories
                .Where(h => h.UserId == userId && h.VocaItemId == vocaItemId)
                .Include(h => h.VocaItem)
                .OrderByDescending(h => h.ReviewedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy lịch sử học tập mới nhất của người dùng với một từ vựng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaItemId">ID từ vựng</param>
        /// <returns>Task<VocaItemHistory> - Bản ghi lịch sử mới nhất hoặc null</returns>
        public async Task<VocaItemHistory?> GetLatestByUserIdAndVocaItemIdAsync(string userId, int vocaItemId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            return await _context.VocaItemHistories
                .Where(h => h.UserId == userId && h.VocaItemId == vocaItemId)
                .OrderByDescending(h => h.ReviewedAt)
                .FirstOrDefaultAsync();
        }

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
        public async Task<VocaItemHistory> UpdateLearningStatusAsync(string userId, int vocaItemId, string status)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            if (string.IsNullOrEmpty(status))
                throw new ArgumentNullException(nameof(status));

            // Kiểm tra từ vựng có tồn tại không
            var vocaItem = await _context.VocaItems.FindAsync(vocaItemId);
            if (vocaItem == null)
                throw new ArgumentException($"VocaItem with ID {vocaItemId} not found");

            // Cập nhật status trong VocaItem nếu cần
            vocaItem.Status = status;

            // Tạo bản ghi lịch sử mới
            var history = new VocaItemHistory
            {
                UserId = userId,
                VocaItemId = vocaItemId,
                Status = status,
                ReviewedAt = DateTime.UtcNow
            };

            _context.VocaItemHistories.Add(history);
            await _context.SaveChangesAsync();

            return history;
        }

        /// <summary>
        /// Đánh dấu nhiều từ vựng đã học trong một lần
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaItemIds">Danh sách ID từ vựng</param>
        /// <param name="status">Trạng thái chung</param>
        /// <returns>Task<IEnumerable<VocaItemHistory>> - Danh sách bản ghi lịch sử vừa tạo</returns>
        public async Task<IEnumerable<VocaItemHistory>> UpdateMultipleLearningStatusAsync(string userId, IEnumerable<int> vocaItemIds, string status)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));
            if (vocaItemIds == null || !vocaItemIds.Any())
                throw new ArgumentNullException(nameof(vocaItemIds));
            if (string.IsNullOrEmpty(status))
                throw new ArgumentNullException(nameof(status));

            var histories = new List<VocaItemHistory>();
            var currentTime = DateTime.UtcNow;

            // Cập nhật status cho các VocaItem
            var vocaItems = await _context.VocaItems
                .Where(v => vocaItemIds.Contains(v.Id))
                .ToListAsync();

            foreach (var vocaItem in vocaItems)
            {
                vocaItem.Status = status;

                // Tạo bản ghi lịch sử
                var history = new VocaItemHistory
                {
                    UserId = userId,
                    VocaItemId = vocaItem.Id,
                    Status = status,
                    ReviewedAt = currentTime
                };

                histories.Add(history);
                _context.VocaItemHistories.Add(history);
            }

            await _context.SaveChangesAsync();
            return histories;
        }

        #endregion

        #region Phương thức thống kê và báo cáo

        /// <summary>
        /// Đếm tổng số từ đã học của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Task<int> - Số lượng từ đã học</returns>
        public async Task<int> CountLearnedWordsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            // Đếm số từ vựng có status "learned" mới nhất
            return await _context.VocaItemHistories
                .Where(h => h.UserId == userId)
                .GroupBy(h => h.VocaItemId)
                .Where(g => g.OrderByDescending(h => h.ReviewedAt).First().Status == "learned")
                .CountAsync();
        }

        /// <summary>
        /// Đếm số từ học trong ngày hôm nay
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Task<int> - Số từ học hôm nay</returns>
        public async Task<int> CountTodayLearnedWordsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            return await _context.VocaItemHistories
                .Where(h => h.UserId == userId &&
                           h.ReviewedAt >= today &&
                           h.ReviewedAt < tomorrow &&
                           h.Status == "learned")
                .Select(h => h.VocaItemId)
                .Distinct()
                .CountAsync();
        }

        /// <summary>
        /// Lấy thống kê học tập của người dùng trong một bộ từ vựng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaSetId">ID bộ từ vựng</param>
        /// <returns>Task<Dictionary<string, int>> - Dictionary chứa thống kê theo trạng thái</returns>
        public async Task<Dictionary<string, int>> GetLearningStatisticsByVocaSetAsync(string userId, int vocaSetId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            // Lấy tất cả từ vựng trong bộ từ vựng
            var vocaItemsInSet = await _context.VocaItems
                .Where(v => v.VocaSetId == vocaSetId)
                .Select(v => v.Id)
                .ToListAsync();

            var totalWords = vocaItemsInSet.Count;

            // Lấy trạng thái mới nhất của từng từ
            var latestStatuses = await _context.VocaItemHistories
                .Where(h => h.UserId == userId && vocaItemsInSet.Contains(h.VocaItemId))
                .GroupBy(h => h.VocaItemId)
                .Select(g => g.OrderByDescending(h => h.ReviewedAt).First().Status)
                .ToListAsync();

            var statistics = new Dictionary<string, int>
            {
                {"total", totalWords},
                {"learned", latestStatuses.Count(s => s == "learned")},
                {"notlearned", totalWords - latestStatuses.Count},
                {"reviewing", latestStatuses.Count(s => s == "reviewing")}
            };

            return statistics;
        }

        /// <summary>
        /// Lấy thống kê tổng quan học tập của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Task<Dictionary<string, int>> - Dictionary chứa các thống kê tổng quan</returns>
        public async Task<Dictionary<string, int>> GetOverallLearningStatisticsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            var totalLearned = await CountLearnedWordsAsync(userId);
            var todayLearned = await CountTodayLearnedWordsAsync(userId);

            // Đếm tổng số từ vựng người dùng có quyền truy cập
            var totalAccessibleWords = await _context.VocaItems
                .Where(v => v.VocaSet.UserId == userId || v.VocaSet.Status != "private")
                .CountAsync();

            // Đếm số ngày liên tiếp học tập
            var learningStreak = await CalculateLearningStreakAsync(userId);

            return new Dictionary<string, int>
            {
                {"totalLearned", totalLearned},
                {"todayLearned", todayLearned},
                {"totalAccessible", totalAccessibleWords},
                {"learningStreak", learningStreak}
            };
        }

        /// <summary>
        /// Lấy lịch sử học tập theo tuần/tháng để hiển thị biểu đồ tiến độ
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="days">Số ngày trước đó cần lấy dữ liệu</param>
        /// <returns>Task<Dictionary<DateTime, int>> - Dictionary với key là ngày và value là số từ học</returns>
        public async Task<Dictionary<DateTime, int>> GetLearningProgressChartDataAsync(string userId, int days = 30)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            var startDate = DateTime.UtcNow.Date.AddDays(-days);
            var endDate = DateTime.UtcNow.Date.AddDays(1);

            var dailyLearningData = await _context.VocaItemHistories
                .Where(h => h.UserId == userId &&
                           h.ReviewedAt >= startDate &&
                           h.ReviewedAt < endDate &&
                           h.Status == "learned")
                .GroupBy(h => h.ReviewedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Select(h => h.VocaItemId).Distinct().Count() })
                .ToDictionaryAsync(x => x.Date, x => x.Count);

            // Đảm bảo có đầy đủ các ngày trong khoảng thời gian
            var result = new Dictionary<DateTime, int>();
            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                result[date] = dailyLearningData.ContainsKey(date) ? dailyLearningData[date] : 0;
            }

            return result;
        }

        #endregion

        #region Phương thức hỗ trợ học tập

        /// <summary>
        /// Lấy danh sách từ vựng cần ôn tập của người dùng
        /// (Dựa trên lịch sử học và thời gian ôn tập)
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="reviewIntervalDays">Khoảng cách ngày ôn tập (mặc định 7 ngày)</param>
        /// <returns>Task<IEnumerable<int>> - Danh sách ID từ vựng cần ôn tập</returns>
        public async Task<IEnumerable<int>> GetVocaItemsNeedReviewAsync(string userId, int reviewIntervalDays = 7)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            var reviewThreshold = DateTime.UtcNow.AddDays(-reviewIntervalDays);

            return await _context.VocaItemHistories
                .Where(h => h.UserId == userId)
                .GroupBy(h => h.VocaItemId)
                .Where(g => g.OrderByDescending(h => h.ReviewedAt).First().Status == "learned" &&
                           g.OrderByDescending(h => h.ReviewedAt).First().ReviewedAt <= reviewThreshold)
                .Select(g => g.Key)
                .ToListAsync();
        }

        /// <summary>
        /// Kiểm tra xem một từ vựng đã được học hay chưa
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaItemId">ID từ vựng</param>
        /// <returns>Task<bool> - True nếu đã học</returns>
        public async Task<bool> IsVocaItemLearnedAsync(string userId, int vocaItemId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            var latestHistory = await GetLatestByUserIdAndVocaItemIdAsync(userId, vocaItemId);
            return latestHistory?.Status == "learned";
        }

        /// <summary>
        /// Lấy trạng thái học tập hiện tại của một từ vựng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <param name="vocaItemId">ID từ vựng</param>
        /// <returns>Task<string> - Trạng thái hiện tại hoặc "notlearned" nếu chưa có lịch sử</returns>
        public async Task<string> GetCurrentLearningStatusAsync(string userId, int vocaItemId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            var latestHistory = await GetLatestByUserIdAndVocaItemIdAsync(userId, vocaItemId);
            return latestHistory?.Status ?? "notlearned";
        }

        #endregion

        #region Phương thức hỗ trợ private

        /// <summary>
        /// Tính toán số ngày liên tiếp học tập của người dùng
        /// </summary>
        /// <param name="userId">ID người dùng</param>
        /// <returns>Task<int> - Số ngày liên tiếp</returns>
        private async Task<int> CalculateLearningStreakAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;
            var streak = 0;
            var currentDate = today;

            while (currentDate >= today.AddDays(-365)) // Giới hạn tìm kiếm 1 năm
            {
                var hasLearningOnDate = await _context.VocaItemHistories
                    .AnyAsync(h => h.UserId == userId &&
                                  h.ReviewedAt.Date == currentDate &&
                                  h.Status == "learned");

                if (hasLearningOnDate)
                {
                    streak++;
                    currentDate = currentDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        #endregion
    }
}