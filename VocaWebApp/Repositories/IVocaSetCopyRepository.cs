using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public interface IVocaSetCopyRepository
    {
        Task<IEnumerable<VocaSetCopy>> GetAllAsync();
        Task<VocaSetCopy> GetByIdAsync(int id);
        Task<IEnumerable<VocaSetCopy>> GetByOriginalSetIdAsync(int originalSetId);
        Task<IEnumerable<VocaSetCopy>> GetByUserIdAsync(string copiedByUserId);
        Task<VocaSetCopy> AddAsync(VocaSetCopy copy);
        Task<bool> DeleteAsync(int id);

        Task<bool> HasUserCopiedAsync(int originalSetId, string copiedByUserId);
    }
}
