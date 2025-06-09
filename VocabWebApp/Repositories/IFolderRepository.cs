using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public interface IFolderRepository
    {
        Task<IEnumerable<Folder>> GetAllAsync();
        Task<Folder> GetByIdAsync(int id);
        Task<IEnumerable<Folder>> GetByUserIdAsync(string userId);
        Task<Folder> AddAsync(Folder folder);
        Task<Folder> UpdateAsync(Folder folder);
        Task<bool> DeleteAsync(int id);
    }
}
