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
        private readonly IVocaItemHistoryRepository _historyRepo;
        private readonly IReviewReminderRepository _reminderRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public VocaSetController(
            IVocaSetRepository setRepo,
            IVocaItemRepository itemRepo,
            IVocaItemHistoryRepository historyRepo,
            IReviewReminderRepository reminderRepo,
            UserManager<ApplicationUser> userManager)
        {
            _setRepo = setRepo;
            _itemRepo = itemRepo;
            _historyRepo = historyRepo;
            _reminderRepo = reminderRepo;
            _userManager = userManager;
        }

        // [GET] /VocaSet/Index
        public async Task<IActionResult> Index(string sortBy = "accessed", bool ascending = false, int page = 1, int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var pagedResult = await _setRepo.GetPagedAsync(page, pageSize, userId, sortBy, !ascending);
            return View(pagedResult);
        }

        // [GET] /VocaSet/Search
        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return RedirectToAction(nameof(Index));
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vocaSets = await _setRepo.SearchAsync(keyword, userId, false);

            var pagedResult = new PagedResult<VocaSet>
            {
                Items = vocaSets.ToList(),
                PageNumber = 1,
                PageSize = vocaSets.Count(),
                TotalCount = vocaSets.Count()
            };

            ViewBag.Keyword = keyword;
            return View("Index", pagedResult);
        }

        // [GET] /VocaSet/Display/{id}
        [HttpGet]
        public async Task<IActionResult> Display(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hasAccess = await _setRepo.HasAccessAsync(id, userId);
            if (!hasAccess) return Forbid();

            var vocaSet = await _setRepo.GetByIdWithIncludesAsync(id, "VocaItems", "ReviewReminders");
            if (vocaSet == null) return NotFound();

            if (vocaSet.UserId != userId)
            {
                await _setRepo.IncrementViewCountAsync(id);
            }

            await _setRepo.UpdateLastAccessedAsync(id);

            return View(vocaSet);
        }

        // [GET] /VocaSet/Create
        public IActionResult Create(int? folderId)
        {
            var model = new VocaSet();
            if (folderId.HasValue)
            {
                model.FolderId = folderId.Value;
            }
            return View(model);
        }

        // [POST] /VocaSet/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VocaSet model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.UserId = userId;
            await _setRepo.AddAsync(model);
            TempData["Success"] = "Tạo bộ từ vựng thành công!";
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }

        // [GET] /VocaSet/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hasPermission = await _setRepo.HasEditPermissionAsync(id, userId);
            if (!hasPermission) return Forbid();

            var vocaSet = await _setRepo.GetByIdAsync(id);
            if (vocaSet == null) return NotFound();

            return View(vocaSet);
        }

        // [POST] /VocaSet/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VocaSet model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hasPermission = await _setRepo.HasEditPermissionAsync(model.Id, userId);
            if (!hasPermission) return Forbid();

            if (!ModelState.IsValid) return View(model);

            await _setRepo.UpdateAsync(model);
            TempData["Success"] = "Cập nhật bộ từ vựng thành công!";
            return RedirectToAction(nameof(Display), new { id = model.Id });
        }

        // [POST] /VocaSet/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hasPermission = await _setRepo.HasEditPermissionAsync(id, userId);
            if (!hasPermission) return Forbid();

            await _setRepo.SoftDeleteAsync(id);
            TempData["Success"] = "Xóa bộ từ vựng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // [GET] /VocaSet/Flashcard/{id}
        [HttpGet]
        public async Task<IActionResult> Flashcard(int id, string status = "notlearned", int count = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hasAccess = await _setRepo.HasAccessAsync(id, userId);
            if (!hasAccess) return Forbid();

            var vocaSet = await _setRepo.GetByIdAsync(id);
            if (vocaSet == null) return NotFound();

            IEnumerable<VocaItem> items;
            if (status == "all")
            {
                var allItems = await _itemRepo.GetByVocaSetIdAsync(id);
                items = allItems.OrderBy(x => Guid.NewGuid()).Take(count);
            }
            else
            {
                items = await _itemRepo.GetByStatusAsync(id, status, count);
            }

            ViewBag.VocaSet = vocaSet;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentCount = count;
            return View(items);
        }

        // [POST] /VocaSet/UpdateFlashcardStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFlashcardStatus([FromBody] FlashcardStatusRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var item = await _itemRepo.GetWithVocaSetAsync(request.VocaItemId);

            if (item == null || item.VocaSet.UserId != userId)
            {
                return Json(new { success = false, message = "Không tìm thấy từ vựng hoặc bạn không có quyền truy cập." });
            }

            var newStatus = request.Known ? "learned" : "reviewing";

            var statusUpdateSuccess = await _itemRepo.UpdateLearningStatusAsync(request.VocaItemId, newStatus);

            if (statusUpdateSuccess)
            {
                await _historyRepo.UpdateLearningStatusAsync(userId, request.VocaItemId, newStatus);
                return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
            }

            return Json(new { success = false, message = "Lỗi khi cập nhật trạng thái." });
        }

        // [GET] /VocaSet/Reminders/{id}
        [HttpGet]
        public async Task<IActionResult> Reminders(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hasPermission = await _setRepo.HasEditPermissionAsync(id, userId);
            if (!hasPermission) return Forbid();

            var vocaSet = await _setRepo.GetByIdAsync(id);
            if (vocaSet == null) return NotFound();

            var reminders = await _reminderRepo.GetByVocaSetIdAsync(id);
            var vm = new RemindersViewModel
            {
                VocaSetId = id,
                VocaSetName = vocaSet.Name,
                Items = reminders
            };
            return View(vm);
        }

        // [POST] /VocaSet/AddReminder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReminder(RemindersViewModel vm)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var hasPermission = await _setRepo.HasEditPermissionAsync(vm.VocaSetId, userId);
            if (!hasPermission) return Forbid();

            if (!ModelState.IsValid)
            {
                // Cần tải lại dữ liệu cho View nếu có lỗi
                var vocaSet = await _setRepo.GetByIdAsync(vm.VocaSetId);
                vm.Items = await _reminderRepo.GetByVocaSetIdAsync(vm.VocaSetId);
                vm.VocaSetName = vocaSet?.Name ?? "";
                return View("Reminders", vm);
            }

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

        // [POST] /VocaSet/Copy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Copy(int id, int? folderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var copiedSet = await _setRepo.CopyVocaSetAsync(id, userId, null, folderId);

            if (copiedSet == null)
            {
                TempData["Error"] = "Không thể sao chép bộ từ này.";
                return RedirectToAction(nameof(Display), new { id });
            }

            TempData["Success"] = "Sao chép bộ từ thành công!";
            return RedirectToAction(nameof(Display), new { id = copiedSet.Id });
        }
    }

    public class FlashcardStatusRequest
    {
        public int VocaItemId { get; set; }
        public bool Known { get; set; }
    }

    public class RemindersViewModel
    {
        public int VocaSetId { get; set; }
        public string VocaSetName { get; set; }
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow.Date;
        public IEnumerable<ReviewReminder> Items { get; set; } = new List<ReviewReminder>();
    }
}
