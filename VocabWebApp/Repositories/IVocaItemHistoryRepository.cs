using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public interface IVocaItemHistoryRepository
    {
        Task<IEnumerable<VocaItemHistory>> GetAllAsync();
        Task<VocaItemHistory> GetByIdAsync(int id);
        Task<IEnumerable<VocaItemHistory>> GetByUserIdAsync(string userId);
        Task<IEnumerable<VocaItemHistory>> GetByVocaItemIdAsync(int vocaItemId);
        Task<VocaItemHistory> AddAsync(VocaItemHistory history);
        Task<VocaItemHistory> UpdateAsync(VocaItemHistory history);
        Task<bool> DeleteAsync(int id);

        Task<VocaItemHistory> GetLatestAsync(string userId, int vocaItemId);
    }
}
