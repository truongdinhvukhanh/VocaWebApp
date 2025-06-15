using Microsoft.EntityFrameworkCore;
using VocaWebApp.Models;

namespace VocaWebApp.Data.Repositories
{
    public class FolderRepository : IFolderRepository
    {
        private readonly ApplicationDbContext _context;

        public FolderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Folder?> GetByIdAsync(int folderId, string userId)
        {
            return await _context.Folders
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId);
        }

        public async Task<IEnumerable<Folder>> GetByUserAsync(string userId,
            string? sortBy = "CreatedAt",
            bool ascending = false,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Folders
                .Where(f => f.UserId == userId)
                .AsQueryable();

            // Xử lý sorting
            query = sortBy?.ToLower() switch
            {
                "name" => ascending ? query.OrderBy(f => f.Name) : query.OrderByDescending(f => f.Name),
                "createdat" => ascending ? query.OrderBy(f => f.CreatedAt) : query.OrderByDescending(f => f.CreatedAt),
                _ => query.OrderByDescending(f => f.CreatedAt)
            };

            // Phân trang
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Folder>> SearchAsync(string userId, string keyword)
        {
            return await _context.Folders
                .Where(f => f.UserId == userId &&
                    (f.Name.Contains(keyword) || f.Description.Contains(keyword)))
                .ToListAsync();
        }

        public async Task<Folder> AddAsync(Folder folder)
        {
            // Kiểm tra trùng tên
            if (await _context.Folders.AnyAsync(f =>
                f.UserId == folder.UserId &&
                f.Name == folder.Name))
            {
                throw new InvalidOperationException("Tên folder đã tồn tại");
            }

            await _context.Folders.AddAsync(folder);
            await _context.SaveChangesAsync();
            return folder;
        }

        public async Task UpdateAsync(Folder folder)
        {
            var existing = await GetByIdAsync(folder.Id, folder.UserId);
            if (existing == null) throw new KeyNotFoundException("Folder không tồn tại");

            _context.Entry(existing).CurrentValues.SetValues(folder);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int folderId, string userId)
        {
            var folder = await GetByIdAsync(folderId, userId);
            if (folder == null) return;

            // Kiểm tra có VocaSet nào không
            if (await _context.VocaSets.AnyAsync(v => v.FolderId == folderId))
            {
                throw new InvalidOperationException("Không thể xóa folder đang chứa bộ từ vựng");
            }

            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CheckNameExistsAsync(string userId, string folderName)
        {
            return await _context.Folders
                .AnyAsync(f => f.UserId == userId && f.Name == folderName);
        }

        public async Task<Folder?> GetWithVocaSetsAsync(int folderId, string userId)
        {
            return await _context.Folders
                .Include(f => f.VocaSets)
                .FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId);
        }
    }

}
