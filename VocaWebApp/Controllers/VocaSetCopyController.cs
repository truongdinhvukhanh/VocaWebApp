using Microsoft.AspNetCore.Mvc;
using VocaWebApp.Models;
using VocaWebApp.Repositories;

namespace VocaWebApp.Controllers
{
    [Route("vocasetcopies")]
    public class VocaSetCopyController : Controller
    {
        private readonly IVocaSetCopyRepository _vocaSetCopyRepository;

        public VocaSetCopyController(IVocaSetCopyRepository vocaSetCopyRepository)
        {
            _vocaSetCopyRepository = vocaSetCopyRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var copies = await _vocaSetCopyRepository.GetAllAsync();
            return View(copies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var copy = await _vocaSetCopyRepository.GetByIdAsync(id);
            if (copy == null)
                return NotFound();
            return View(copy);
        }

        [HttpGet("original/{originalSetId}")]
        public async Task<IActionResult> ByOriginal(int originalSetId)
        {
            var copies = await _vocaSetCopyRepository.GetByOriginalSetIdAsync(originalSetId);
            return View(copies);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> ByUser(string userId)
        {
            var copies = await _vocaSetCopyRepository.GetByUserIdAsync(userId);
            return View(copies);
        }

        [HttpGet("create")]
        public IActionResult Create() => View();

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VocaSetCopy copy)
        {
            if (ModelState.IsValid)
            {
                await _vocaSetCopyRepository.AddAsync(copy);
                return RedirectToAction(nameof(Index));
            }
            return View(copy);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var copy = await _vocaSetCopyRepository.GetByIdAsync(id);
            if (copy == null)
                return NotFound();
            return View(copy);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _vocaSetCopyRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("has-copied")]
        public async Task<IActionResult> HasCopied(int originalSetId, string userId)
        {
            bool hasCopied = await _vocaSetCopyRepository.HasUserCopiedAsync(originalSetId, userId);
            return Json(new { hasCopied });
        }
    }
}