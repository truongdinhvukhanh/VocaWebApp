using Microsoft.EntityFrameworkCore;
using VocaWebApp.Data;
using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public class VocaItemHistoryRepository : IVocaItemHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public VocaItemHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VocaItemHistory>> GetAllAsync()
        {
            return await _context.VocaItemHistories.ToListAsync();
        }

        public async Task<VocaItemHistory> GetByIdAsync(int id)
        {
            return await _context.VocaItemHistories.FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<IEnumerable<VocaItemHistory>> GetByUserIdAsync(string userId)
        {
            return await _context.VocaItemHistories.Where(h => h.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<VocaItemHistory>> GetByVocaItemIdAsync(int vocaItemId)
        {
            return await _context.VocaItemHistories.Where(h => h.VocaItemId == vocaItemId).ToListAsync();
        }

        public async Task<VocaItemHistory> AddAsync(VocaItemHistory history)
        {
            _context.VocaItemHistories.Add(history);
            await _context.SaveChangesAsync();
            return history;
        }

        public async Task<VocaItemHistory> UpdateAsync(VocaItemHistory history)
        {
            _context.VocaItemHistories.Update(history);
            await _context.SaveChangesAsync();
            return history;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var history = await _context.VocaItemHistories.FindAsync(id);
            if (history == null) return false;
            _context.VocaItemHistories.Remove(history);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<VocaItemHistory> GetLatestAsync(string userId, int vocaItemId)
        {
            return await _context.VocaItemHistories
                .Where(h => h.UserId == userId && h.VocaItemId == vocaItemId)
                .OrderByDescending(h => h.ReviewedAt)
                .FirstOrDefaultAsync();
        }
    }
}
