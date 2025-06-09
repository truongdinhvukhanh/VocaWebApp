using Microsoft.EntityFrameworkCore;
using VocaWebApp.Data;
using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public class VocaSetCopyRepository : IVocaSetCopyRepository
    {
        private readonly ApplicationDbContext _context;

        public VocaSetCopyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VocaSetCopy>> GetAllAsync()
        {
            return await _context.VocaSetCopies.ToListAsync();
        }

        public async Task<VocaSetCopy> GetByIdAsync(int id)
        {
            return await _context.VocaSetCopies.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<VocaSetCopy>> GetByOriginalSetIdAsync(int originalSetId)
        {
            return await _context.VocaSetCopies.Where(c => c.OriginalSetId == originalSetId).ToListAsync();
        }

        public async Task<IEnumerable<VocaSetCopy>> GetByUserIdAsync(string copiedByUserId)
        {
            return await _context.VocaSetCopies.Where(c => c.CopiedByUserId == copiedByUserId).ToListAsync();
        }

        public async Task<VocaSetCopy> AddAsync(VocaSetCopy copy)
        {
            _context.VocaSetCopies.Add(copy);
            await _context.SaveChangesAsync();
            return copy;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var copy = await _context.VocaSetCopies.FindAsync(id);
            if (copy == null) return false;
            _context.VocaSetCopies.Remove(copy);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasUserCopiedAsync(int originalSetId, string copiedByUserId)
        {
            return await _context.VocaSetCopies.AnyAsync(c => c.OriginalSetId == originalSetId && c.CopiedByUserId == copiedByUserId);
        }
    }
}
