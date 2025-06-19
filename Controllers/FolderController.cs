using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VocaWebApp.Data.Repositories;
using VocaWebApp.Models;

namespace VocaWebApp.Controllers
{
    /// <summary>
    /// Controller quản lý folder tổ chức bộ từ vựng. Triển khai các tính năng tạo, chỉnh sửa, xóa folder, quản lý bộ từ vựng trong folder.
    /// </summary>
    [Authorize]
    public class FolderController : Controller
    {
        private readonly IFolderRepository folderRepository;
        private readonly IVocaSetRepository vocaSetRepository;
        private readonly UserManager<ApplicationUser> userManager;

        public FolderController(IFolderRepository folderRepository, IVocaSetRepository vocaSetRepository, UserManager<ApplicationUser> userManager)
        {
            this.folderRepository = folderRepository;
            this.vocaSetRepository = vocaSetRepository;
            this.userManager = userManager;
        }

        /// <summary>
        /// Hiển thị danh sách folder của user với sắp xếp và phân trang. (Trang: Bộ từ vựng của tôi / Folder)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string? sortBy = "CreatedAt", bool ascending = false, int page = 1, int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                // GetByUserAsync already handles pagination and sorting
                var folders = await folderRepository.GetByUserAsync(userId, sortBy, ascending, page, pageSize);

                var totalFolders = await CountUserFoldersAsync(userId);
                var totalPages = (int)Math.Ceiling((double)totalFolders / pageSize);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.Ascending = ascending;
                ViewBag.HasPreviousPage = (page > 1);
                ViewBag.HasNextPage = (page < totalPages);
                ViewBag.SearchKeyword = null; // Clear search keyword when displaying full index
                ViewBag.ActiveTab = "folders"; // Set active tab

                return View(folders);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách folder.";
                return View(new List<Folder>());
            }
        }

        /// <summary>
        /// Hiển thị tất cả VocaSets của user với sắp xếp và phân trang. (Tab VocaSets)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> VocaSets(string? sortBy = "LastAccessed", bool ascending = false, int page = 1, int pageSize = 20, int? folderId = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                // Get all VocaSets for the user
                var vocaSets = await vocaSetRepository.GetByUserIdAsync(userId);

                // Filter by folder if specified
                if (folderId.HasValue)
                {
                    vocaSets = vocaSets.Where(v => v.FolderId == folderId.Value);
                }

                // Apply sorting
                var sortedVocaSets = SortVocaSets(vocaSets, sortBy, ascending);

                // Apply pagination
                var pagedVocaSets = sortedVocaSets
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var totalVocaSets = vocaSets.Count();
                var totalPages = (int)Math.Ceiling((double)totalVocaSets / pageSize);

                // Get folders for filter dropdown
                var folders = await folderRepository.GetByUserAsync(userId, "Name", true, 1, int.MaxValue);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.Ascending = ascending;
                ViewBag.HasPreviousPage = (page > 1);
                ViewBag.HasNextPage = (page < totalPages);
                ViewBag.SearchKeyword = null;
                ViewBag.ActiveTab = "vocasets"; // Set active tab
                ViewBag.FolderId = folderId;
                ViewBag.Folders = folders;

                return View("Index", pagedVocaSets);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách bộ từ vựng.";
                return View("Index", new List<VocaSet>());
            }
        }

        /// <summary>
        /// Tìm kiếm folder theo tên và mô tả. (Thanh tìm kiếm theo tên bộ từ, từ khóa)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Search(string? keyword, string? sortBy = "CreatedAt", bool ascending = false, int page = 1, int pageSize = 20)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                // If keyword is empty or whitespace, redirect to Index to show all folders with existing pagination/sort
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return RedirectToAction("Index", new { sortBy, ascending, page, pageSize });
                }

                // SearchAsync returns ALL matching folders, not paginated
                var folders = await folderRepository.SearchAsync(userId, keyword);

                // Apply sorting to the search results
                var sortedFolders = folders;
                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "name":
                            sortedFolders = ascending ? sortedFolders.OrderBy(f => f.Name) : sortedFolders.OrderByDescending(f => f.Name);
                            break;
                        case "createdat":
                            sortedFolders = ascending ? sortedFolders.OrderBy(f => f.CreatedAt) : sortedFolders.OrderByDescending(f => f.CreatedAt);
                            break;
                        default:
                            sortedFolders = sortedFolders.OrderByDescending(f => f.CreatedAt); // Default sort
                            break;
                    }
                }

                // Apply pagination to the sorted search results
                var pagedFolders = sortedFolders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var totalFolders = folders.Count(); // Total count of search results (before pagination)
                var totalPages = (int)Math.Ceiling((double)totalFolders / pageSize);

                // Set ViewBag values for Index.cshtml to display search results correctly with pagination/sorting
                ViewBag.SearchKeyword = keyword;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.Ascending = ascending;
                ViewBag.HasPreviousPage = (page > 1);
                ViewBag.HasNextPage = (page < totalPages);
                ViewBag.ActiveTab = "folders";

                return View("Index", pagedFolders); // Render Index view with filtered and paginated search results
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tìm kiếm folder.";
                // Redirect to Index, passing existing sort/pagination to maintain state
                return RedirectToAction("Index", new { sortBy, ascending, page, pageSize });
            }
        }

        /// <summary>
        /// Tìm kiếm VocaSets theo tên và mô tả. (Tab VocaSets)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SearchVocaSets(string? keyword, string? sortBy = "LastAccessed", bool ascending = false, int page = 1, int pageSize = 20, int? folderId = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                // If keyword is empty or whitespace, redirect to VocaSets to show all VocaSets
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return RedirectToAction("VocaSets", new { sortBy, ascending, page, pageSize, folderId });
                }

                // Get all VocaSets for the user
                var vocaSets = await vocaSetRepository.GetByUserIdAsync(userId);

                // Apply search filter
                vocaSets = vocaSets.Where(v =>
                    v.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(v.Description) && v.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(v.Keywords) && v.Keywords.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                );

                // Filter by folder if specified
                if (folderId.HasValue)
                {
                    vocaSets = vocaSets.Where(v => v.FolderId == folderId.Value);
                }

                // Apply sorting
                var sortedVocaSets = SortVocaSets(vocaSets, sortBy, ascending);

                // Apply pagination
                var pagedVocaSets = sortedVocaSets.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var totalVocaSets = vocaSets.Count();
                var totalPages = (int)Math.Ceiling((double)totalVocaSets / pageSize);

                // Get folders for filter dropdown
                var folders = await folderRepository.GetByUserAsync(userId, "Name", true, 1, int.MaxValue);

                ViewBag.SearchKeyword = keyword;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.PageSize = pageSize;
                ViewBag.SortBy = sortBy;
                ViewBag.Ascending = ascending;
                ViewBag.HasPreviousPage = (page > 1);
                ViewBag.HasNextPage = (page < totalPages);
                ViewBag.ActiveTab = "vocasets";
                ViewBag.FolderId = folderId;
                ViewBag.Folders = folders;

                return View("Index", pagedVocaSets);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tìm kiếm bộ từ vựng.";
                return RedirectToAction("VocaSets", new { sortBy, ascending, page, pageSize, folderId });
            }
        }

        /// <summary>
        /// Hiển thị form tạo folder mới. (Tạo, chỉnh sửa, xóa folder, quản lý bộ từ vựng)
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Folder());
        }

        /// <summary>
        /// Xử lý tạo folder mới. (Tạo, chỉnh sửa, xóa folder, quản lý bộ từ vựng)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Folder model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                if (!ModelState.IsValid)
                    return View(model);

                // Kiểm tra tên folder tồn tại chưa
                if (await folderRepository.CheckNameExistsAsync(userId, model.Name))
                {
                    ModelState.AddModelError("Name", "Tên folder đã tồn tại.");
                    return View(model);
                }

                // Tạo folder mới
                model.UserId = userId;
                model.CreatedAt = DateTime.UtcNow;
                await folderRepository.AddAsync(model);
                TempData["SuccessMessage"] = "Tạo folder thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo folder.";
                return View(model);
            }
        }

        /// <summary>
        /// Hiển thị form chỉnh sửa folder. (Tạo, chỉnh sửa, xóa folder, quản lý bộ từ vựng)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                var folder = await folderRepository.GetByIdAsync(id, userId);
                if (folder == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy folder hoặc bạn không có quyền truy cập.";
                    return RedirectToAction(nameof(Index));
                }
                return View(folder);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin folder.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Xử lý cập nhật thông tin folder. (Tạo, chỉnh sửa, xóa folder, quản lý bộ từ vựng)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Folder model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                if (id != model.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(model);

                // Đảm bảo folder thuộc về user hiện tại
                model.UserId = userId;
                await folderRepository.UpdateAsync(model);
                TempData["SuccessMessage"] = "Cập nhật folder thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException ex)
            {
                TempData["ErrorMessage"] = "Không tìm thấy folder hoặc bạn không có quyền truy cập.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật folder.";
                return View(model);
            }
        }

        /// <summary>
        /// Hiển thị chi tiết folder và danh sách bộ từ vựng bên trong. (Xem danh sách bộ từ vựng trong từng folder)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int id, string? sortBy = "LastAccessed", bool ascending = false, int page = 1, int pageSize = 12)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                var folder = await folderRepository.GetWithVocaSetsAsync(id, userId);
                if (folder == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy folder hoặc bạn không có quyền truy cập.";
                    return RedirectToAction(nameof(Index));
                }

                // Lấy danh sách VocaSet trong folder với phân trang và sắp xếp
                var vocaSets = await vocaSetRepository.GetByUserAndFolderAsync(userId, id);

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
                ViewBag.HasPreviousPage = (page > 1);
                ViewBag.HasNextPage = (page < ViewBag.TotalPages);

                return View(pagedVocaSets);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải chi tiết folder.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Xóa folder có kiểm tra folder có chứa bộ từ vựng không. (Tạo, chỉnh sửa, xóa folder, quản lý bộ từ vựng)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                await folderRepository.DeleteAsync(id, userId);
                TempData["SuccessMessage"] = "Xóa folder thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa folder.";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Xác nhận xóa folder - hiển thị thông tin trước khi xóa.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return RedirectToAction("Login", "Account");

                var folder = await folderRepository.GetWithVocaSetsAsync(id, userId);
                if (folder == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy folder hoặc bạn không có quyền truy cập.";
                    return RedirectToAction(nameof(Index));
                }
                return View(folder);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin folder.";
                return RedirectToAction(nameof(Index));
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Đếm tổng số folder của user dùng cho phân trang
        /// </summary>
        private async Task<int> CountUserFoldersAsync(string userId)
        {
            try
            {
                // GetByUserAsync without page/pageSize will return all folders for the user.
                var folders = await folderRepository.GetByUserAsync(userId, "CreatedAt", false, 1, int.MaxValue);
                return folders.Count();
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Sắp xếp danh sách VocaSet theo yêu cầu (Sắp xếp bộ từ vựng theo truy cập gần đây, thời gian truy cập lần cuối, số từ đã học/chưa học)
        /// </summary>
        private IEnumerable<VocaSet> SortVocaSets(IEnumerable<VocaSet> vocaSets, string? sortBy, bool ascending)
        {
            return sortBy?.ToLower() switch
            {
                "name" => ascending ? vocaSets.OrderBy(v => v.Name) : vocaSets.OrderByDescending(v => v.Name),
                "createdat" => ascending ? vocaSets.OrderBy(v => v.CreatedAt) : vocaSets.OrderByDescending(v => v.CreatedAt),
                "lastaccessed" => ascending ? vocaSets.OrderBy(v => v.LastAccessed ?? DateTime.MinValue) : vocaSets.OrderByDescending(v => v.LastAccessed ?? DateTime.MinValue),
                "viewcount" => ascending ? vocaSets.OrderBy(v => v.ViewCount) : vocaSets.OrderByDescending(v => v.ViewCount),
                "folder" => ascending ? vocaSets.OrderBy(v => v.Folder?.Name ?? "Chưa phân loại") : vocaSets.OrderByDescending(v => v.Folder?.Name ?? "Chưa phân loại"),
                _ => vocaSets.OrderByDescending(v => v.LastAccessed ?? v.CreatedAt), // Mặc định truy cập gần đây
            };
        }
        #endregion
    }
}
