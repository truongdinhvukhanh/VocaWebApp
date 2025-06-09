using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public interface IVocaSetRepository
    {
        Task<IEnumerable<VocaSet>> GetAllAsync();
        Task<VocaSet> GetByIdAsync(int id);
        Task<IEnumerable<VocaSet>> GetByUserIdAsync(string userId);
        Task<VocaSet> AddAsync(VocaSet vocaSet);
        Task<VocaSet> UpdateAsync(VocaSet vocaSet);
        Task<bool> DeleteAsync(int id);

        // Lấy các từ chưa thuộc để ôn tập flashcard
        Task<IEnumerable<VocaItem>> GetFlashcardsAsync(int setId, string userId);
    }
}