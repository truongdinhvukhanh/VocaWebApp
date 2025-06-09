using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public interface IAuditLogRepository
    {
        Task<IEnumerable<AuditLog>> GetAllAsync();
        Task<AuditLog> GetByIdAsync(int id);
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(string userId);
        Task<AuditLog> AddAsync(AuditLog log);
        Task<bool> DeleteAsync(int id);
    }
}
