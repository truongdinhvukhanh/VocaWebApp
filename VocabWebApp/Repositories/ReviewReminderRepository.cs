using Microsoft.EntityFrameworkCore;
using VocaWebApp.Data;
using VocaWebApp.Models;

namespace VocaWebApp.Repositories
{
    public class ReviewReminderRepository : IReviewReminderRepository
    {
        private readonly ApplicationDbContext _context;

        public ReviewReminderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReviewReminder>> GetAllAsync()
        {
            return await _context.ReviewReminders.ToListAsync();
        }

        public async Task<ReviewReminder> GetByIdAsync(int id)
        {
            return await _context.ReviewReminders.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<ReviewReminder>> GetByUserIdAsync(string userId)
        {
            return await _context.ReviewReminders.Where(r => r.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<ReviewReminder>> GetByVocaSetIdAsync(int vocaSetId)
        {
            return await _context.ReviewReminders.Where(r => r.VocaSetId == vocaSetId).ToListAsync();
        }

        public async Task<ReviewReminder> AddAsync(ReviewReminder reminder)
        {
            _context.ReviewReminders.Add(reminder);
            await _context.SaveChangesAsync();
            return reminder;
        }

        public async Task<ReviewReminder> UpdateAsync(ReviewReminder reminder)
        {
            _context.ReviewReminders.Update(reminder);
            await _context.SaveChangesAsync();
            return reminder;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var reminder = await _context.ReviewReminders.FindAsync(id);
            if (reminder == null) return false;
            _context.ReviewReminders.Remove(reminder);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<ReviewReminder>> GetPendingRemindersAsync()
        {
            return await _context.ReviewReminders
                .Where(r => !r.IsSent && r.ReviewDate <= System.DateTime.Now)
                .ToListAsync();
        }
    }
}
