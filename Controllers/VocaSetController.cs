using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VocaWebApp.Data.Repositories;
using VocaWebApp.Models;

namespace VocaWebApp.Controllers
{
    [Authorize]
    public class VocaSetController : Controller
    {
        private readonly IVocaSetRepository _setRepo;
        private readonly IVocaItemRepository _itemRepo;
        private readonly IReviewReminderRepository _reminderRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public VocaSetController(
            IVocaSetRepository setRepo,
            IVocaItemRepository itemRepo,
            IReviewReminderRepository reminderRepo,
            UserManager<ApplicationUser> userManager)
        {
            _setRepo = setRepo;
            _itemRepo = itemRepo;
            _reminderRepo = reminderRepo;
            _userManager = userManager;
        }

        // 1. Create new VocaSet
        // GET: Hiển thị form tạo mới với folderId tùy chọn
        [HttpGet]
        public IActionResult Create(int? folderId)
        {
            var model = new VocaSet();
            if (folderId.HasValue)
            {
                model.FolderId = folderId.Value;
            }
            return View(model);
        }

        // POST: Tạo mới VocaSet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VocaSet model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.UserId = userId;
            model.CreatedAt = DateTime.UtcNow;

            await _setRepo.AddAsync(model);
            TempData["Success"] = "Tạo bộ từ vựng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // 2. List & search user's VocaSets (Dashboard)
        [HttpGet]
        public async Task<IActionResult> Index(string sortBy = "CreatedAt", bool ascending = false, int page = 1, int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var paged = await _setRepo.GetPagedAsync(page, pageSize, userId, sortBy, !ascending);
            return View(paged);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return RedirectToAction(nameof(Index));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var results = await _setRepo.SearchAsync(keyword, userId, false);
            ViewBag.Keyword = keyword;
            return View("Index", results);
        }

        // 3. Display VocaSet details
        [HttpGet]
        public async Task<IActionResult> Display(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var set = await _setRepo.GetByIdWithIncludesAsync(id, "VocaItems", "ReviewReminders");
            if (set == null || set.UserId != userId) return Forbid();

            return View(set);
        }

        // 4. Edit VocaSet metadata
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var set = await _setRepo.GetByIdAsync(id);
            return View(set);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VocaSet model)
        {
            if (!ModelState.IsValid) return View(model);

            await _setRepo.UpdateAsync(model);
            TempData["Success"] = "Cập nhật bộ từ vựng thành công!";
            return RedirectToAction(nameof(Display), new { id = model.Id });
        }

        // 5. Delete (soft) VocaSet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _setRepo.SoftDeleteAsync(id);
            TempData["Success"] = "Xóa bộ từ vựng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // 6. Flashcard study - FIXED VERSION
        [HttpGet]
        public async Task<IActionResult> Flashcard(int id, string status = "not_learned", int count = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vocaSet = await _setRepo.GetByIdAsync(id);

            if (vocaSet == null || vocaSet.UserId != userId)
            {
                return Forbid(); // Hoặc NotFound()
            }

            IEnumerable<VocaItem> items;

            // Lấy từ vựng dựa trên trạng thái người dùng chọn
            if (status == "all")
            {
                // Lấy ngẫu nhiên từ tất cả các từ trong bộ
                var allItems = await _itemRepo.GetByVocaSetIdAsync(id);
                items = allItems.OrderBy(x => Guid.NewGuid()).Take(count);
            }
            else
            {
                // Lấy từ theo trạng thái cụ thể
                items = await _itemRepo.GetByStatusAsync(id, status, count);
            }

            // Luôn trả về view, kể cả khi `items` rỗng
            // View sẽ tự xử lý việc hiển thị thông báo
            ViewBag.VocaSet = vocaSet;
            ViewBag.CurrentStatus = status; // Truyền trạng thái hiện tại sang view để active filter
            ViewBag.CurrentCount = count;   // Truyền số lượng hiện tại sang view

            return View(items);
        }

        // 7. UpdateFlashcardStatus - FIXED VERSION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFlashcardStatus([FromBody] FlashcardStatusRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _itemRepo.GetWithVocaSetAsync(request.VocaItemId);

            // Kiểm tra quyền sở hữu
            if (item == null || item.VocaSet.UserId != userId)
            {
                return Json(new { success = false, message = "Không tìm thấy từ vựng hoặc bạn không có quyền truy cập." });
            }

            // Cập nhật trạng thái dựa vào việc người dùng có biết từ không
            var newStatus = request.Known ? "learned" : "reviewing"; // Nếu biết -> đã học, không biết -> cần ôn tập
            var success = await _itemRepo.UpdateLearningStatusAsync(request.VocaItemId, newStatus);

            if (success)
            {
                // Có thể ghi lại lịch sử học tập ở đây nếu cần
                return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
            }

            return Json(new { success = false, message = "Lỗi khi cập nhật trạng thái." });
        }

        // 8. Review reminders
        [HttpGet]
        public async Task<IActionResult> Reminders(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reminders = await _reminderRepo.GetByVocaSetIdAsync(id);
            var vm = new RemindersViewModel
            {
                VocaSetId = id,
                Items = reminders
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReminder(RemindersViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var model = new ReviewReminder
            {
                VocaSetId = vm.VocaSetId,
                ReviewDate = vm.ReviewDate,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };
            await _reminderRepo.CreateAsync(model);
            TempData["Success"] = "Thêm lịch nhắc ôn tập thành công!";
            return RedirectToAction(nameof(Reminders), new { id = vm.VocaSetId });
        }

        // 9. Copy public VocaSet
        [HttpPost]
        public async Task<IActionResult> Copy(int id, int? folderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var copy = await _setRepo.CopyVocaSetAsync(id, userId, null, folderId);
            return RedirectToAction(nameof(Display), new { id = copy.Id });
        }

        // 10. Admin: list all sets & manage visibility/status
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminIndex()
        {
            var all = await _setRepo.GetAllAsync();
            return View(all);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, string status, bool isHidden)
        {
            await _setRepo.UpdateStatusAsync(id, status);
            await _setRepo.UpdateVisibilityAsync(id, isHidden);
            return RedirectToAction(nameof(AdminIndex));
        }
    }

    // ViewModel cho Reminders
    public class RemindersViewModel
    {
        public int VocaSetId { get; set; }              // ID của bộ từ
        public DateTime ReviewDate { get; set; }       // Ngày ôn tập mới
        public IEnumerable<ReviewReminder> Items { get; set; }  // Danh sách reminders cũ
    }

    // Request model cho UpdateFlashcardStatus
    public class FlashcardStatusRequest
    {
        public int VocaItemId { get; set; }
        public bool Known { get; set; }
    }
}