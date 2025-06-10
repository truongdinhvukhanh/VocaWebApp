using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VocaWebApp.Models;
using VocaWebApp.Repositories;

namespace VocaWebApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IVocaItemRepository _vocaItemRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(IVocaItemRepository vocaItemRepository, UserManager<ApplicationUser> userManager)
        {
            _vocaItemRepository = vocaItemRepository;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // Hoặc RedirectToAction("Login", "Account");
            }
            var vocaItems = await _vocaItemRepository.GetByUserAsync(user.Id);

            // Giả sử thuộc tính đánh dấu đã học là IsLearned
            int learnedCount = vocaItems.Count(item => item.Status == "learned");

            ViewBag.LearnedVocaCount = learnedCount;

            return View();
        }
    }
}
