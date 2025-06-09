using Microsoft.AspNetCore.Mvc;
using VocaWebApp.Models;
using VocaWebApp.Repositories;

namespace VocaWebApp.Controllers
{
    [Route("vocaitemhistories")]
    public class VocaItemHistoryController : Controller
    {
        private readonly IVocaItemHistoryRepository _historyRepository;

        public VocaItemHistoryController(IVocaItemHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var histories = await _historyRepository.GetAllAsync();
            return View(histories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var history = await _historyRepository.GetByIdAsync(id);
            if (history == null)
                return NotFound();
            return View(history);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> ByUser(string userId)
        {
            var histories = await _historyRepository.GetByUserIdAsync(userId);
            return View(histories);
        }

        [HttpGet("vocaitem/{vocaItemId}")]
        public async Task<IActionResult> ByVocaItem(int vocaItemId)
        {
            var histories = await _historyRepository.GetByVocaItemIdAsync(vocaItemId);
            return View(histories);
        }

        [HttpGet("create")]
        public IActionResult Create() => View();

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VocaItemHistory history)
        {
            if (ModelState.IsValid)
            {
                await _historyRepository.AddAsync(history);
                return RedirectToAction(nameof(Index));
            }
            return View(history);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var history = await _historyRepository.GetByIdAsync(id);
            if (history == null)
                return NotFound();
            return View(history);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VocaItemHistory history)
        {
            if (id != history.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _historyRepository.UpdateAsync(history);
                return RedirectToAction(nameof(Index));
            }
            return View(history);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var history = await _historyRepository.GetByIdAsync(id);
            if (history == null)
                return NotFound();
            return View(history);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _historyRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("latest")]
        public async Task<IActionResult> Latest(string userId, int vocaItemId)
        {
            var history = await _historyRepository.GetLatestAsync(userId, vocaItemId);
            return View(history);
        }
    }
}