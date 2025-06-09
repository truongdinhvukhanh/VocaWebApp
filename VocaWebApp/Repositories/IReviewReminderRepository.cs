using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public interface IReviewReminderRepository
    {
        Task<IEnumerable<ReviewReminder>> GetAllAsync();
        Task<ReviewReminder> GetByIdAsync(int id);
        Task<IEnumerable<ReviewReminder>> GetByUserIdAsync(string userId);
        Task<IEnumerable<ReviewReminder>> GetByVocaSetIdAsync(int vocaSetId);
        Task<ReviewReminder> AddAsync(ReviewReminder reminder);
        Task<ReviewReminder> UpdateAsync(ReviewReminder reminder);
        Task<bool> DeleteAsync(int id);

        Task<IEnumerable<ReviewReminder>> GetPendingRemindersAsync();
    }
}
