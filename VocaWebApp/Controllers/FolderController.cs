using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VocaWebApp.Models;
using VocaWebApp.Repositories;

namespace VocaWebApp.Controllers
{
    [Route("folders")]
    public class FolderController : Controller
    {
        private readonly IFolderRepository _folderRepository;

        public FolderController(IFolderRepository folderRepository)
        {
            _folderRepository = folderRepository;
        }

        // GET: /folders
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var folders = await _folderRepository.GetAllAsync();
            return View(folders);
        }

        // GET: /folders/details/5
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var folder = await _folderRepository.GetByIdAsync(id);
            if (folder == null)
                return NotFound();

            return View(folder);
        }

        // GET: /folders/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /folders/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Folder folder)
        {
            if (ModelState.IsValid)
            {
                await _folderRepository.AddAsync(folder);
                return RedirectToAction(nameof(Index));
            }
            return View(folder);
        }

        // GET: /folders/edit/5
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var folder = await _folderRepository.GetByIdAsync(id);
            if (folder == null)
                return NotFound();

            return View(folder);
        }

        // POST: /folders/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Folder folder)
        {
            if (id != folder.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _folderRepository.UpdateAsync(folder);
                return RedirectToAction(nameof(Index));
            }
            return View(folder);
        }

        // GET: /folders/delete/5
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var folder = await _folderRepository.GetByIdAsync(id);
            if (folder == null)
                return NotFound();

            return View(folder);
        }

        // POST: /folders/delete/5
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _folderRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}