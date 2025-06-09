using Microsoft.AspNetCore.Mvc;
using VocaWebApp.Models;
using VocaWebApp.Repositories;

namespace VocaWebApp.Controllers
{
    [Route("vocasets")]
    public class VocaSetController : Controller
    {
        private readonly IVocaSetRepository _vocaSetRepository;

        public VocaSetController(IVocaSetRepository vocaSetRepository)
        {
            _vocaSetRepository = vocaSetRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var sets = await _vocaSetRepository.GetAllAsync();
            return View(sets);
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var set = await _vocaSetRepository.GetByIdAsync(id);
            if (set == null)
                return NotFound();
            return View(set);
        }

        [HttpGet("create")]
        public IActionResult Create() => View();

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VocaSet set)
        {
            if (ModelState.IsValid)
            {
                await _vocaSetRepository.AddAsync(set);
                return RedirectToAction(nameof(Index));
            }
            return View(set);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var set = await _vocaSetRepository.GetByIdAsync(id);
            if (set == null)
                return NotFound();
            return View(set);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VocaSet set)
        {
            if (id != set.Id)
                return BadRequest();
            if (ModelState.IsValid)
            {
                await _vocaSetRepository.UpdateAsync(set);
                return RedirectToAction(nameof(Index));
            }
            return View(set);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var set = await _vocaSetRepository.GetByIdAsync(id);
            if (set == null)
                return NotFound();
            return View(set);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _vocaSetRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Lấy flashcards để ôn tập
        [HttpGet("{setId}/flashcards")]
        public async Task<IActionResult> Flashcards(int setId, string userId)
        {
            var items = await _vocaSetRepository.GetFlashcardsAsync(setId, userId);
            return View(items);
        }
    }
}