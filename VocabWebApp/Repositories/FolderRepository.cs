using Microsoft.EntityFrameworkCore;
using VocaWebApp.Data;
using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public class FolderRepository : IFolderRepository
    {
        private readonly ApplicationDbContext _context;

        public FolderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Folder>> GetAllAsync()
        {
            return await _context.Folders.ToListAsync();
        }

        public async Task<Folder> GetByIdAsync(int id)
        {
            return await _context.Folders.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<IEnumerable<Folder>> GetByUserIdAsync(string userId)
        {
            return await _context.Folders.Where(f => f.UserId == userId).ToListAsync();
        }

        public async Task<Folder> AddAsync(Folder folder)
        {
            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();
            return folder;
        }

        public async Task<Folder> UpdateAsync(Folder folder)
        {
            _context.Folders.Update(folder);
            await _context.SaveChangesAsync();
            return folder;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var folder = await _context.Folders.FindAsync(id);
            if (folder == null) return false;
            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
