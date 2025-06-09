using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VocaWebApp.Models;
using VocaWebApp.Repositories;

namespace VocaWebApp.Controllers
{
    [Route("reviewreminders")]
    public class ReviewReminderController : Controller
    {
        private readonly IReviewReminderRepository _reminderRepository;

        public ReviewReminderController(IReviewReminderRepository reminderRepository)
        {
            _reminderRepository = reminderRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reminders = await _reminderRepository.GetAllAsync();
            return View(reminders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);
            if (reminder == null)
                return NotFound();
            return View(reminder);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> ByUser(string userId)
        {
            var reminders = await _reminderRepository.GetByUserIdAsync(userId);
            return View(reminders);
        }

        [HttpGet("vocaset/{vocaSetId}")]
        public async Task<IActionResult> ByVocaSet(int vocaSetId)
        {
            var reminders = await _reminderRepository.GetByVocaSetIdAsync(vocaSetId);
            return View(reminders);
        }

        [HttpGet("create")]
        public IActionResult Create() => View();

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReviewReminder reminder)
        {
            if (ModelState.IsValid)
            {
                await _reminderRepository.AddAsync(reminder);
                return RedirectToAction(nameof(Index));
            }
            return View(reminder);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);
            if (reminder == null)
                return NotFound();
            return View(reminder);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReviewReminder reminder)
        {
            if (id != reminder.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _reminderRepository.UpdateAsync(reminder);
                return RedirectToAction(nameof(Index));
            }
            return View(reminder);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var reminder = await _reminderRepository.GetByIdAsync(id);
            if (reminder == null)
                return NotFound();
            return View(reminder);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _reminderRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("pending")]
        public async Task<IActionResult> Pending()
        {
            var reminders = await _reminderRepository.GetPendingRemindersAsync();
            return View(reminders);
        }
    }
}