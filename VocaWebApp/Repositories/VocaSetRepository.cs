using Microsoft.EntityFrameworkCore;
using VocaWebApp.Data;
using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public class VocaSetRepository : IVocaSetRepository
    {
        private readonly ApplicationDbContext _context;

        public VocaSetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VocaSet>> GetAllAsync()
        {
            return await _context.VocaSets.Include(vs => vs.VocaItems).ToListAsync();
        }

        public async Task<VocaSet> GetByIdAsync(int id)
        {
            return await _context.VocaSets.Include(vs => vs.VocaItems)
                .FirstOrDefaultAsync(vs => vs.Id == id);
        }

        public async Task<IEnumerable<VocaSet>> GetByUserIdAsync(string userId)
        {
            return await _context.VocaSets
                .Where(vs => vs.UserId == userId)
                .Include(vs => vs.VocaItems)
                .ToListAsync();
        }

        public async Task<VocaSet> AddAsync(VocaSet vocaSet)
        {
            _context.VocaSets.Add(vocaSet);
            await _context.SaveChangesAsync();
            return vocaSet;
        }

        public async Task<VocaSet> UpdateAsync(VocaSet vocaSet)
        {
            _context.VocaSets.Update(vocaSet);
            await _context.SaveChangesAsync();
            return vocaSet;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var vocaSet = await _context.VocaSets.FindAsync(id);
            if (vocaSet == null) return false;
            _context.VocaSets.Remove(vocaSet);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<VocaItem>> GetFlashcardsAsync(int setId, string userId)
        {
            // Lấy các từ chưa thuộc để ôn tập flashcard
            return await _context.VocaItems
                .Where(vi => vi.VocaSetId == setId && vi.Status != "learned")
                .ToListAsync();
        }
    }
}