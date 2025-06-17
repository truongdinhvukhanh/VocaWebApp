using Microsoft.EntityFrameworkCore;
using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    /// <summary>
    /// Implementation của IVocaItemRepository sử dụng Entity Framework Core
    /// Quản lý các thao tác CRUD và tính năng học từ vựng cho VocaItem
    /// </summary>
    public class VocaItemRepository : IVocaItemRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor khởi tạo repository với ApplicationDbContext
        /// </summary>
        public VocaItemRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Các phương thức CRUD cơ bản

        /// <summary>
        /// Lấy tất cả VocaItem trong hệ thống
        /// Sử dụng cho các tính năng admin quản lý toàn bộ từ vựng
        /// </summary>
        public async Task<IEnumerable<VocaItem>> GetAllAsync()
        {
            return await _context.VocaItems
                .OrderBy(v => v.Word)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy VocaItem theo ID
        /// Sử dụng để hiển thị chi tiết từ vựng, chỉnh sửa
        /// </summary>
        public async Task<VocaItem?> GetByIdAsync(int id)
        {
            return await _context.VocaItems
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        /// <summary>
        /// Thêm VocaItem mới vào cơ sở dữ liệu
        /// Sử dụng khi user tạo từ vựng mới trong bộ từ vựng
        /// </summary>
        public async Task<VocaItem> AddAsync(VocaItem vocaItem)
        {
            if (vocaItem == null)
                throw new ArgumentNullException(nameof(vocaItem));

            _context.VocaItems.Add(vocaItem);
            await _context.SaveChangesAsync();
            return vocaItem;
        }

        /// <summary>
        /// Cập nhật thông tin VocaItem
        /// Sử dụng khi user chỉnh sửa từ vựng, cập nhật trạng thái học tập
        /// </summary>
        public async Task<VocaItem> UpdateAsync(VocaItem vocaItem)
        {
            if (vocaItem == null)
                throw new ArgumentNullException(nameof(vocaItem));

            _context.VocaItems.Update(vocaItem);
            await _context.SaveChangesAsync();
            return vocaItem;
        }

        /// <summary>
        /// Xóa VocaItem khỏi cơ sở dữ liệu theo ID
        /// Sử dụng khi user xóa từ vựng không cần thiết
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var vocaItem = await _context.VocaItems.FindAsync(id);
            if (vocaItem == null)
                return false;

            _context.VocaItems.Remove(vocaItem);
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Các phương thức lấy dữ liệu theo VocaSet

        /// <summary>
        /// Lấy tất cả VocaItem thuộc một VocaSet cụ thể
        /// Sử dụng để hiển thị danh sách từ vựng trong bộ từ, trang chi tiết VocaSet
        /// </summary>
        public async Task<IEnumerable<VocaItem>> GetByVocaSetIdAsync(int vocaSetId)
        {
            return await _context.VocaItems
                .Where(v => v.VocaSetId == vocaSetId)
                .OrderBy(v => v.Word)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy VocaItem với phân trang theo VocaSet
        /// Sử dụng để hiển thị danh sách từ vựng với phân trang, tối ưu hiệu suất
        /// </summary>
        public async Task<IEnumerable<VocaItem>> GetByVocaSetIdWithPaginationAsync(int vocaSetId, int pageNumber, int pageSize)
        {
            return await _context.VocaItems
                .Where(v => v.VocaSetId == vocaSetId)
                .OrderBy(v => v.Word)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Đếm tổng số VocaItem trong một VocaSet
        /// Sử dụng để tính toán phân trang, hiển thị thống kê
        /// </summary>
        public async Task<int> GetCountByVocaSetIdAsync(int vocaSetId)
        {
            return await _context.VocaItems
                .Where(v => v.VocaSetId == vocaSetId)
                .CountAsync();
        }

        #endregion

        #region Các phương thức cho tính năng học tập và flashcard

        /// <summary>
        /// Lấy VocaItem theo trạng thái học tập - FIXED VERSION
        /// Sử dụng để lọc từ vựng theo tiến độ học: đã học, chưa học, đang ôn tập
        /// </summary>
        public async Task<IEnumerable<VocaItem>> GetByStatusAsync(int vocaSetId, string status, int count = 20)
        {
            var query = _context.VocaItems
                .Where(v => v.VocaSetId == vocaSetId && v.Status == status);

            // Lấy ngẫu nhiên 'count' từ bằng cách sắp xếp theo một Guid mới
            // Đây là một kỹ thuật phổ biến và hiệu quả để lấy dòng ngẫu nhiên trong EF Core
            return await query.OrderBy(r => Guid.NewGuid())
                              .Take(count)
                              .ToListAsync();
        }

        /// <summary>
        /// Overload method để tương thích với code cũ - CHỈ 2 THAM SỐ
        /// </summary>
        public async Task<IEnumerable<VocaItem>> GetByStatusAsync(int vocaSetId, string status)
        {
            return await _context.VocaItems
                .Where(v => v.VocaSetId == vocaSetId && v.Status == status)
                .OrderBy(v => v.Word)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy VocaItem chưa học để luyện tập flashcard
        /// Sử dụng trong tính năng flashcard để ưu tiên từ chưa học
        /// </summary>
        public async Task<IEnumerable<VocaItem>> GetUnlearnedForFlashcardAsync(int vocaSetId, int limit)
        {
            return await _context.VocaItems
                .Where(v => v.VocaSetId == vocaSetId && v.Status == "not_learned")
                .Take(limit)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy VocaItem ngẫu nhiên để luyện tập flashcard
        /// Sử dụng trong chế độ flashcard ngẫu nhiên để tăng tính thử thách
        /// </summary>
        public async Task<IEnumerable<VocaItem>> GetRandomForFlashcardAsync(int vocaSetId, int count, bool includeOnlyUnlearned = false)
        {
            var query = _context.VocaItems.Where(v => v.VocaSetId == vocaSetId);

            if (includeOnlyUnlearned)
            {
                query = query.Where(v => v.Status == "not_learned");
            }

            // Sử dụng NEWID() cho SQL Server hoặc RANDOM() cho SQLite để random
            return await query
                .OrderBy(x => Guid.NewGuid())
                .Take(count)
                .ToListAsync();
        }

        /// <summary>
        /// Cập nhật trạng thái học tập của VocaItem
        /// Sử dụng khi user đánh dấu từ đã học/chưa học trong flashcard
        /// </summary>
        public async Task<bool> UpdateLearningStatusAsync(int vocaItemId, string newStatus)
        {
            var vocaItem = await _context.VocaItems.FindAsync(vocaItemId);
            if (vocaItem == null)
                return false;

            vocaItem.Status = newStatus;
            await _context.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Các phương thức tìm kiếm

        /// <summary>
        /// Tìm kiếm VocaItem theo từ khóa trong một VocaSet
        /// Sử dụng cho thanh tìm kiếm trong bộ từ vựng
        /// </summary>
        public async Task<IEnumerable<VocaItem>> SearchInVocaSetAsync(int vocaSetId, string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return await GetByVocaSetIdAsync(vocaSetId);

            keyword = keyword.ToLower().Trim();

            return await _context.VocaItems
                .Where(v => v.VocaSetId == vocaSetId &&
                           (v.Word.ToLower().Contains(keyword) ||
                            (v.Meaning != null && v.Meaning.ToLower().Contains(keyword)) ||
                            (v.ExampleSentence != null && v.ExampleSentence.ToLower().Contains(keyword))))
                .OrderBy(v => v.Word)
                .ToListAsync();
        }

        /// <summary>
        /// Tìm kiếm VocaItem theo từ vựng chính xác
        /// Sử dụng để kiểm tra từ trùng lặp, tìm từ cụ thể
        /// </summary>
        public async Task<VocaItem?> FindByWordAsync(int vocaSetId, string word)
        {
            return await _context.VocaItems
                .FirstOrDefaultAsync(v => v.VocaSetId == vocaSetId &&
                                         v.Word.ToLower() == word.ToLower());
        }

        #endregion

        #region Các phương thức thống kê và báo cáo

        /// <summary>
        /// Lấy thống kê học tập của một VocaSet
        /// Sử dụng để hiển thị báo cáo tiến độ học tập trên dashboard
        /// </summary>
        public async Task<Dictionary<string, int>> GetLearningStatisticsAsync(int vocaSetId)
        {
            var stats = await _context.VocaItems
                .Where(v => v.VocaSetId == vocaSetId)
                .GroupBy(v => v.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<string, int>
            {
                ["learned"] = 0,
                ["not_learned"] = 0,
                ["reviewing"] = 0,
                ["total"] = 0
            };

            foreach (var stat in stats)
            {
                if (stat.Status != null)
                    result[stat.Status] = stat.Count;
                result["total"] += stat.Count;
            }

            return result;
        }

        /// <summary>
        /// Lấy tiến độ học tập theo phần trăm
        /// Sử dụng để hiển thị progress bar, theo dõi mục tiêu học tập
        /// </summary>
        public async Task<double> GetLearningProgressPercentageAsync(int vocaSetId)
        {
            var total = await _context.VocaItems
                .Where(v => v.VocaSetId == vocaSetId)
                .CountAsync();

            if (total == 0)
                return 0;

            var learned = await _context.VocaItems
                .Where(v => v.VocaSetId == vocaSetId && v.Status == "learned")
                .CountAsync();

            return Math.Round((double)learned / total * 100, 2);
        }

        #endregion

        #region Các phương thức hỗ trợ bulk operations

        /// <summary>
        /// Thêm nhiều VocaItem cùng lúc
        /// Sử dụng khi import từ vựng từ file, copy bộ từ vựng
        /// </summary>
        public async Task<IEnumerable<VocaItem>> AddRangeAsync(IEnumerable<VocaItem> vocaItems)
        {
            if (vocaItems == null || !vocaItems.Any())
                return new List<VocaItem>();

            _context.VocaItems.AddRange(vocaItems);
            await _context.SaveChangesAsync();
            return vocaItems;
        }

        /// <summary>
        /// Xóa tất cả VocaItem trong một VocaSet
        /// Sử dụng khi xóa toàn bộ bộ từ vựng
        /// </summary>
        public async Task<int> DeleteByVocaSetIdAsync(int vocaSetId)
        {
            var vocaItems = await _context.VocaItems
                .Where(v => v.VocaSetId == vocaSetId)
                .ToListAsync();

            if (!vocaItems.Any())
                return 0;

            _context.VocaItems.RemoveRange(vocaItems);
            await _context.SaveChangesAsync();
            return vocaItems.Count;
        }

        #endregion

        #region Các phương thức với Include relationships

        /// <summary>
        /// Lấy VocaItem với thông tin VocaSet
        /// Sử dụng khi cần hiển thị thông tin bộ từ vựng cùng với từ vựng
        /// </summary>
        public async Task<VocaItem?> GetWithVocaSetAsync(int id)
        {
            return await _context.VocaItems
                .Include(v => v.VocaSet)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        /// <summary>
        /// Lấy VocaItem với lịch sử học tập
        /// Sử dụng để theo dõi tiến trình học của user với từ vựng cụ thể
        /// </summary>
        public async Task<VocaItem?> GetWithHistoriesAsync(int id)
        {
            return await _context.VocaItems
                .Include(v => v.Histories)
                .ThenInclude(h => h.User)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        #endregion
    }
}