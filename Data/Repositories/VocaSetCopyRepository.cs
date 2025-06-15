using Microsoft.EntityFrameworkCore;
using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    /// <summary>
    /// Triển khai cụ thể của IVocaSetCopyRepository
    /// Cung cấp các phương thức để quản lý việc sao chép bộ từ vựng trong cơ sở dữ liệu
    /// </summary>
    public class VocaSetCopyRepository : IVocaSetCopyRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Constructor khởi tạo repository với ApplicationDbContext
        /// </summary>
        /// <param name="context">Database context để tương tác với cơ sở dữ liệu</param>
        public VocaSetCopyRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Tạo một bản sao mới của bộ từ vựng
        /// Thêm record mới vào bảng VocaSetCopy để ghi lại việc copy
        /// </summary>
        /// <param name="originalSetId">ID của bộ từ vựng gốc cần copy</param>
        /// <param name="copiedByUserId">ID của người dùng thực hiện copy</param>
        /// <returns>VocaSetCopy entity đã được tạo và lưu vào database</returns>
        public async Task<VocaSetCopy> CreateCopyAsync(int originalSetId, string copiedByUserId)
        {
            var vocaSetCopy = new VocaSetCopy
            {
                OriginalSetId = originalSetId,
                CopiedByUserId = copiedByUserId,
                CopiedAt = DateTime.UtcNow
            };

            _context.VocaSetCopies.Add(vocaSetCopy);
            await _context.SaveChangesAsync();

            return vocaSetCopy;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một bản copy theo ID
        /// Bao gồm thông tin liên kết với bộ từ vựng gốc và người copy
        /// </summary>
        /// <param name="id">ID của bản copy</param>
        /// <returns>VocaSetCopy entity với thông tin đầy đủ hoặc null nếu không tìm thấy</returns>
        public async Task<VocaSetCopy?> GetByIdAsync(int id)
        {
            return await _context.VocaSetCopies
                .Include(vsc => vsc.OriginalSet) // Bao gồm thông tin bộ từ vựng gốc
                .Include(vsc => vsc.CopiedByUser) // Bao gồm thông tin người copy
                .FirstOrDefaultAsync(vsc => vsc.Id == id);
        }

        /// <summary>
        /// Lấy danh sách tất cả các bản copy của một bộ từ vựng cụ thể
        /// Sắp xếp theo thời gian copy mới nhất trước
        /// </summary>
        /// <param name="originalSetId">ID của bộ từ vựng gốc</param>
        /// <returns>Danh sách các bản copy được sắp xếp theo thời gian</returns>
        public async Task<IEnumerable<VocaSetCopy>> GetCopiesByOriginalSetIdAsync(int originalSetId)
        {
            return await _context.VocaSetCopies
                .Include(vsc => vsc.CopiedByUser) // Bao gồm thông tin người copy
                .Where(vsc => vsc.OriginalSetId == originalSetId)
                .OrderByDescending(vsc => vsc.CopiedAt) // Sắp xếp theo thời gian copy mới nhất
                .ToListAsync();
        }

        /// <summary>
        /// Lấy danh sách tất cả các bộ từ vựng mà một người dùng đã copy
        /// Bao gồm thông tin của bộ từ vựng gốc để hiển thị
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>Danh sách các bản copy của người dùng được sắp xếp theo thời gian</returns>
        public async Task<IEnumerable<VocaSetCopy>> GetCopiesByUserIdAsync(string userId)
        {
            return await _context.VocaSetCopies
                .Include(vsc => vsc.OriginalSet) // Bao gồm thông tin bộ từ vựng gốc
                .Where(vsc => vsc.CopiedByUserId == userId)
                .OrderByDescending(vsc => vsc.CopiedAt) // Sắp xếp theo thời gian copy mới nhất
                .ToListAsync();
        }

        /// <summary>
        /// Kiểm tra xem một người dùng đã copy một bộ từ vựng cụ thể chưa
        /// Tránh việc copy trùng lặp và hiển thị trạng thái phù hợp trong UI
        /// </summary>
        /// <param name="originalSetId">ID của bộ từ vựng gốc</param>
        /// <param name="userId">ID của người dùng</param>
        /// <returns>True nếu đã copy, False nếu chưa copy</returns>
        public async Task<bool> HasUserCopiedSetAsync(int originalSetId, string userId)
        {
            return await _context.VocaSetCopies
                .AnyAsync(vsc => vsc.OriginalSetId == originalSetId && vsc.CopiedByUserId == userId);
        }

        /// <summary>
        /// Lấy danh sách các bản copy mới nhất trong hệ thống
        /// Sử dụng cho dashboard admin để theo dõi hoạt động copy
        /// </summary>
        /// <param name="count">Số lượng bản copy muốn lấy (mặc định 10)</param>
        /// <returns>Danh sách các bản copy mới nhất với thông tin đầy đủ</returns>
        public async Task<IEnumerable<VocaSetCopy>> GetRecentCopiesAsync(int count = 10)
        {
            return await _context.VocaSetCopies
                .Include(vsc => vsc.OriginalSet) // Bao gồm thông tin bộ từ vựng gốc
                .Include(vsc => vsc.CopiedByUser) // Bao gồm thông tin người copy
                .OrderByDescending(vsc => vsc.CopiedAt) // Sắp xếp theo thời gian mới nhất
                .Take(count) // Lấy số lượng theo yêu cầu
                .ToListAsync();
        }

        /// <summary>
        /// Đếm số lượng bản copy của một bộ từ vựng cụ thể
        /// Sử dụng để hiển thị độ phổ biến và thống kê
        /// </summary>
        /// <param name="originalSetId">ID của bộ từ vựng gốc</param>
        /// <returns>Số lượng bản copy (integer)</returns>
        public async Task<int> GetCopyCountByOriginalSetIdAsync(int originalSetId)
        {
            return await _context.VocaSetCopies
                .CountAsync(vsc => vsc.OriginalSetId == originalSetId);
        }

        /// <summary>
        /// Lấy danh sách các bộ từ vựng được copy nhiều nhất
        /// Trả về thông tin bộ từ vựng cùng với số lần copy
        /// </summary>
        /// <param name="count">Số lượng bộ từ vựng muốn lấy (mặc định 10)</param>
        /// <returns>Danh sách anonymous object chứa thông tin bộ từ vựng và số lần copy</returns>
        public async Task<IEnumerable<object>> GetMostCopiedSetsAsync(int count = 10)
        {
            return await _context.VocaSetCopies
                .Include(vsc => vsc.OriginalSet) // Bao gồm thông tin bộ từ vựng gốc
                .GroupBy(vsc => vsc.OriginalSetId) // Nhóm theo ID bộ từ vựng gốc
                .Select(g => new
                {
                    OriginalSetId = g.Key,
                    OriginalSet = g.First().OriginalSet, // Thông tin bộ từ vựng
                    CopyCount = g.Count(), // Số lần copy
                    LastCopiedAt = g.Max(vsc => vsc.CopiedAt) // Thời gian copy gần nhất
                })
                .OrderByDescending(x => x.CopyCount) // Sắp xếp theo số lần copy giảm dần
                .Take(count) // Lấy số lượng theo yêu cầu
                .ToListAsync();
        }

        /// <summary>
        /// Xóa một bản copy khỏi hệ thống
        /// Thực hiện hard delete từ cơ sở dữ liệu
        /// </summary>
        /// <param name="id">ID của bản copy cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy record</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            var vocaSetCopy = await _context.VocaSetCopies.FindAsync(id);
            if (vocaSetCopy == null)
            {
                return false; // Không tìm thấy bản copy để xóa
            }

            _context.VocaSetCopies.Remove(vocaSetCopy);
            await _context.SaveChangesAsync();
            return true; // Xóa thành công
        }

        /// <summary>
        /// Lấy thống kê số lượng copy theo khoảng thời gian
        /// Sử dụng cho báo cáo và phân tích hoạt động hệ thống
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu của khoảng thời gian</param>
        /// <param name="endDate">Ngày kết thúc của khoảng thời gian</param>
        /// <returns>Số lượng copy trong khoảng thời gian chỉ định</returns>
        public async Task<int> GetCopyCountByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.VocaSetCopies
                .Where(vsc => vsc.CopiedAt >= startDate && vsc.CopiedAt <= endDate)
                .CountAsync();
        }
    }
}