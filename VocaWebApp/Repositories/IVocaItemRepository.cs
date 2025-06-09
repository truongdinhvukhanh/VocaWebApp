using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public interface IVocaItemRepository
    {
        Task<VocaItem> GetByIdAsync(int id);
        Task<IEnumerable<VocaItem>> GetBySetIdAsync(int setId);
        Task<VocaItem> AddAsync(VocaItem vocaItem);
        Task<VocaItem> UpdateAsync(VocaItem vocaItem);
        Task<bool> DeleteAsync(int id);

        // Lấy tất cả các từ thuộc các bộ từ vựng của user
        Task<IEnumerable<VocaItem>> GetByUserAsync(string userId);
        // Cập nhật trạng thái từ
        Task<VocaItem> UpdateStatusAsync(int vocaItemId, string status);
    }
}
