using Microsoft.EntityFrameworkCore;
using VocaWebApp.Data;
using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public class VocaItemRepository : IVocaItemRepository
    {
        private readonly ApplicationDbContext _context;

        public VocaItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VocaItem> GetByIdAsync(int id)
        {
            return await _context.VocaItems.FirstOrDefaultAsync(vi => vi.Id == id);
        }

        public async Task<IEnumerable<VocaItem>> GetBySetIdAsync(int setId)
        {
            return await _context.VocaItems
                .Where(vi => vi.VocaSetId == setId)
                .ToListAsync();
        }

        public async Task<VocaItem> AddAsync(VocaItem vocaItem)
        {
            _context.VocaItems.Add(vocaItem);
            await _context.SaveChangesAsync();
            return vocaItem;
        }

        public async Task<VocaItem> UpdateAsync(VocaItem vocaItem)
        {
            _context.VocaItems.Update(vocaItem);
            await _context.SaveChangesAsync();
            return vocaItem;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var vocaItem = await _context.VocaItems.FindAsync(id);
            if (vocaItem == null) return false;
            _context.VocaItems.Remove(vocaItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<VocaItem>> GetByUserAsync(string userId)
        {
            // Lấy tất cả các từ thuộc các bộ từ vựng của user
            return await (from vi in _context.VocaItems
                          join vs in _context.VocaSets on vi.VocaSetId equals vs.Id
                          where vs.UserId == userId
                          select vi).ToListAsync();
        }

        public async Task<VocaItem> UpdateStatusAsync(int vocaItemId, string status)
        {
            var vocaItem = await _context.VocaItems.FirstOrDefaultAsync(vi => vi.Id == vocaItemId);
            if (vocaItem == null) return null;
            vocaItem.Status = status;
            _context.VocaItems.Update(vocaItem);
            await _context.SaveChangesAsync();
            return vocaItem;
        }
    }
}
