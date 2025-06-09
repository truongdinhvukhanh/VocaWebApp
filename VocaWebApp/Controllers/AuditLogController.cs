using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VocaWebApp.Models;
using VocaWebApp.Repositories;

namespace VocaWebApp.Controllers
{
    [Route("auditlogs")]
    public class AuditLogController : Controller
    {
        private readonly IAuditLogRepository _auditLogRepository;

        public AuditLogController(IAuditLogRepository auditLogRepository)
        {
            _auditLogRepository = auditLogRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var logs = await _auditLogRepository.GetAllAsync();
            return View(logs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var log = await _auditLogRepository.GetByIdAsync(id);
            if (log == null)
                return NotFound();
            return View(log);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> ByUser(string userId)
        {
            var logs = await _auditLogRepository.GetByUserIdAsync(userId);
            return View(logs);
        }

        [HttpGet("create")]
        public IActionResult Create() => View();

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AuditLog log)
        {
            if (ModelState.IsValid)
            {
                await _auditLogRepository.AddAsync(log);
                return RedirectToAction(nameof(Index));
            }
            return View(log);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var log = await _auditLogRepository.GetByIdAsync(id);
            if (log == null)
                return NotFound();
            return View(log);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _auditLogRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}