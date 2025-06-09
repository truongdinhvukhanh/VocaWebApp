using Microsoft.EntityFrameworkCore;
using VocaWebApp.Data;
using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly ApplicationDbContext _context;

        public AuditLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync()
        {
            return await _context.AuditLogs.ToListAsync();
        }

        public async Task<AuditLog> GetByIdAsync(int id)
        {
            return await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId)
        {
            return await _context.AuditLogs.Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task<AuditLog> AddAsync(AuditLog log)
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
            return log;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null) return false;
            _context.AuditLogs.Remove(log);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
