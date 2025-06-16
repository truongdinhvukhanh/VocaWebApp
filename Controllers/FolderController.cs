using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VocaWebApp.Data.Repositories;
using VocaWebApp.Models;
using System.Security.Claims;

namespace VocaWebApp.Controllers
{
    /// <summary>
    /// Controller quản lý folder tổ chức bộ từ vựng
    /// Triển khai các tính năng: tạo, chỉnh sửa, xóa folder; quản lý bộ từ vựng trong folder
    /// </summary>
    [Authorize]
    public class FolderController : Controller
    {
        private readonly IFolderRepository _folderRepository;
        private readonly IVocaSetRepository _vocaSetRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public FolderController(
            IFolderRepository folderRepository,
            IVocaSetRepository vocaSetRepository,
            UserManager<ApplicationUser> userManager)
        {
            _folderRepository = folderRepository;
            _vocaSetRepository = vocaSetRepository;
            _userManager = userManager;
        }

        /// <summary>
        /// Hiển thị danh sách folder của user với sắp xếp và phân trang
        /// Trang 4: Bộ từ vựng của tôi (Folder)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? sortBy = "CreatedAt", bool ascending = false, int page = 1, int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var folders = await _folderRepository.GetByUserAsync(userId, sortBy, ascending, page, pageSize);

                // Tính toán thông tin phân trang
                var totalFolders = await CountUserFoldersAsync(userId);
                var totalPages = (int)Math.Ceiling((double)totalFolders / pageSize);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.Ascending = ascending;
                ViewBag.HasPreviousPage = page > 1;
                ViewBag.HasNextPage = page < totalPages;

                return View(folders);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách folder.";
                return View(new List<Folder>());
            }
        }

        /// <summary>
        /// Tìm kiếm folder theo tên và mô tả
        /// Thanh tìm kiếm theo tên bộ từ, từ khóa
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return RedirectToAction("Index");
                }

                var folders = await _folderRepository.SearchAsync(userId, keyword);
                ViewBag.SearchKeyword = keyword;

                return View("Index", folders);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tìm kiếm folder.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Hiển thị form tạo folder mới
        /// Tạo, chỉnh sửa, xóa folder quản lý bộ từ vựng
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Folder());
        }

        /// <summary>
        /// Xử lý tạo folder mới
        /// Tạo, chỉnh sửa, xóa folder quản lý bộ từ vựng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Folder model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Kiểm tra tên folder đã tồn tại chưa
                if (await _folderRepository.CheckNameExistsAsync(userId, model.Name))
                {
                    ModelState.AddModelError("Name", "Tên folder đã tồn tại.");
                    return View(model);
                }

                // Tạo folder mới
                model.UserId = userId;
                model.CreatedAt = DateTime.UtcNow;

                await _folderRepository.AddAsync(model);

                TempData["SuccessMessage"] = "Tạo folder thành công!";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo folder.";
                return View(model);
            }
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa folder
        /// Tạo, chỉnh sửa, xóa folder quản lý bộ từ vựng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var folder = await _folderRepository.GetByIdAsync(id, userId);
                if (folder == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy folder hoặc bạn không có quyền truy cập.";
                    return RedirectToAction("Index");
                }

                return View(folder);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin folder.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Xử lý cập nhật thông tin folder
        /// Tạo, chỉnh sửa, xóa folder quản lý bộ từ vựng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Folder model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                if (id != model.Id)
                {
                    return BadRequest();
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Đảm bảo folder thuộc về user hiện tại
                model.UserId = userId;

                await _folderRepository.UpdateAsync(model);

                TempData["SuccessMessage"] = "Cập nhật folder thành công!";
                return RedirectToAction("Index");
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = "Không tìm thấy folder hoặc bạn không có quyền truy cập.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật folder.";
                return View(model);
            }
        }

        /// <summary>
        /// Hiển thị chi tiết folder và danh sách bộ từ vựng bên trong
        /// Xem danh sách bộ từ vựng trong từng folder
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id, string? sortBy = "LastAccessed", bool ascending = false, int page = 1, int pageSize = 12)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var folder = await _folderRepository.GetWithVocaSetsAsync(id, userId);
                if (folder == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy folder hoặc bạn không có quyền truy cập.";
                    return RedirectToAction("Index");
                }

                // Lấy danh sách VocaSet trong folder với phân trang và sắp xếp
                var vocaSets = await _vocaSetRepository.GetByUserAndFolderAsync(userId, id);

                // Sắp xếp VocaSet theo yêu cầu
                var sortedVocaSets = SortVocaSets(vocaSets, sortBy, ascending);

                // Phân trang
                var pagedVocaSets = sortedVocaSets
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Folder = folder;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)vocaSets.Count() / pageSize);
                ViewBag.PageSize = pageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.Ascending = ascending;
                ViewBag.HasPreviousPage = page > 1;
                ViewBag.HasNextPage = page < ViewBag.TotalPages;

                return View(pagedVocaSets);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải chi tiết folder.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Xóa folder (có kiểm tra folder có chứa bộ từ vựng không)
        /// Tạo, chỉnh sửa, xóa folder quản lý bộ từ vựng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                await _folderRepository.DeleteAsync(id, userId);

                TempData["SuccessMessage"] = "Xóa folder thành công!";
                return RedirectToAction("Index");
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa folder.";
                return RedirectToAction("Index");
            }
        }

        /// <summary>
        /// Xác nhận xóa folder - hiển thị thông tin trước khi xóa
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var folder = await _folderRepository.GetWithVocaSetsAsync(id, userId);
                if (folder == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy folder hoặc bạn không có quyền truy cập.";
                    return RedirectToAction("Index");
                }

                return View(folder);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin folder.";
                return RedirectToAction("Index");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Đếm tổng số folder của user (dùng cho phân trang)
        /// </summary>
        private async Task<int> CountUserFoldersAsync(string userId)
        {
            try
            {
                var folders = await _folderRepository.GetByUserAsync(userId, "CreatedAt", false, 1, int.MaxValue);
                return folders.Count();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Sắp xếp danh sách VocaSet theo yêu cầu
        /// Sắp xếp bộ từ vựng theo truy cập gần đây, thời gian truy cập lần cuối, số từ học/chưa học
        /// </summary>
        private IEnumerable<VocaSet> SortVocaSets(IEnumerable<VocaSet> vocaSets, string? sortBy, bool ascending)
        {
            return sortBy?.ToLower() switch
            {
                "name" => ascending ? vocaSets.OrderBy(v => v.Name) : vocaSets.OrderByDescending(v => v.Name),
                "createdat" => ascending ? vocaSets.OrderBy(v => v.CreatedAt) : vocaSets.OrderByDescending(v => v.CreatedAt),
                "lastaccessed" => ascending ? vocaSets.OrderBy(v => v.LastAccessed ?? DateTime.MinValue) : vocaSets.OrderByDescending(v => v.LastAccessed ?? DateTime.MinValue),
                "viewcount" => ascending ? vocaSets.OrderBy(v => v.ViewCount) : vocaSets.OrderByDescending(v => v.ViewCount),
                _ => vocaSets.OrderByDescending(v => v.LastAccessed ?? v.CreatedAt) // Mặc định: truy cập gần đây
            };
        }

        #endregion
    }
}