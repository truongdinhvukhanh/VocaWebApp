using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VocaWebApp.Data.Repositories;
using VocaWebApp.Models;

namespace VocaWebApp.Controllers
{
    /// <summary>
    /// Controller quản lý các tính năng từ vựng
    /// Triển khai CRUD và các chức năng học tập cho VocaItem
    /// </summary>
    [Authorize]
    public class VocaItemController : Controller
    {
        private readonly IVocaItemRepository _itemRepo;
        private readonly IVocaSetRepository _setRepo;
        private readonly IVocaItemHistoryRepository _historyRepo;
        private readonly ILogger<VocaItemController> _logger;

        public VocaItemController(
            IVocaItemRepository itemRepo,
            IVocaSetRepository setRepo,
            IVocaItemHistoryRepository historyRepo,
            ILogger<VocaItemController> logger)
        {
            _itemRepo = itemRepo;
            _setRepo = setRepo;
            _historyRepo = historyRepo;
            _logger = logger;
        }

        #region AJAX endpoints for VocaSet/Create & VocaSet/Edit

        /// <summary>
        /// Lấy danh sách từ vựng trong một bộ từ vựng để hiển thị trong form chỉnh sửa
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetItemsByVocaSetId(int vocaSetId, string keyword = "", string status = "")
        {
            try
            {
                // Kiểm tra quyền sở hữu bộ từ vựng
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var vocaSet = await _setRepo.GetByIdWithIncludesAsync(vocaSetId, "VocaItems");

                if (vocaSet == null || vocaSet.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to access VocaSet {VocaSetId} without permission", userId, vocaSetId);
                    return Json(new { success = false, message = "Không tìm thấy bộ từ vựng hoặc bạn không có quyền truy cập." });
                }

                var vocaItems = await _itemRepo.GetByVocaSetIdAsync(vocaSetId);

                // Apply filters if provided
                if (!string.IsNullOrEmpty(keyword))
                {
                    vocaItems = vocaItems.Where(i =>
                        i.Word.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        (i.Meaning != null && i.Meaning.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(status))
                {
                    vocaItems = vocaItems.Where(i => i.Status == status).ToList();
                }

                return Json(new { success = true, data = vocaItems });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading VocaItems for VocaSet {VocaSetId}", vocaSetId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách từ vựng." });
            }
        }

        /// <summary>
        /// Thêm từ vựng mới vào bộ từ vựng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem(VocaItem vocaItem)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();

                    _logger.LogWarning("Invalid model state for AddItem: {Errors}", string.Join(", ", errors.SelectMany(e => e.Errors)));
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
                }

                // Kiểm tra quyền sở hữu bộ từ vựng
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var vocaSet = await _setRepo.GetByIdAsync(vocaItem.VocaSetId);

                if (vocaSet == null || vocaSet.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to add item to VocaSet {VocaSetId} without permission", userId, vocaItem.VocaSetId);
                    return Json(new { success = false, message = "Không tìm thấy bộ từ vựng hoặc bạn không có quyền truy cập." });
                }

                // Kiểm tra trùng lặp từ vựng trong bộ
                var existingItem = await _itemRepo.FindByWordAsync(vocaItem.VocaSetId, vocaItem.Word);
                if (existingItem != null)
                {
                    return Json(new { success = false, message = $"Từ vựng '{vocaItem.Word}' đã tồn tại trong bộ từ vựng này." });
                }

                // Thiết lập trạng thái mặc định
                vocaItem.Status = "notlearned";

                // Thêm từ vựng mới
                var newItem = await _itemRepo.AddAsync(vocaItem);
                _logger.LogInformation("User {UserId} added new VocaItem {ItemId} to VocaSet {VocaSetId}", userId, newItem.Id, vocaItem.VocaSetId);

                return Json(new { success = true, data = newItem, message = "Thêm từ vựng thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding VocaItem to VocaSet {VocaSetId}", vocaItem.VocaSetId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi thêm từ vựng." });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một từ vựng
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetItemById(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var vocaItem = await _itemRepo.GetWithVocaSetAsync(id);

                if (vocaItem == null || vocaItem.VocaSet.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to get VocaItem {ItemId} without permission", userId, id);
                    return Json(new { success = false, message = "Không tìm thấy từ vựng hoặc bạn không có quyền truy cập." });
                }

                return Json(new { success = true, data = vocaItem });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting VocaItem {ItemId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải thông tin từ vựng." });
            }
        }

        /// <summary>
        /// Cập nhật thông tin từ vựng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItem(VocaItem vocaItem)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();

                    _logger.LogWarning("Invalid model state for UpdateItem: {Errors}", string.Join(", ", errors.SelectMany(e => e.Errors)));
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
                }

                // Kiểm tra quyền sở hữu và tồn tại
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingItem = await _itemRepo.GetWithVocaSetAsync(vocaItem.Id);

                if (existingItem == null || existingItem.VocaSet.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to update VocaItem {ItemId} without permission", userId, vocaItem.Id);
                    return Json(new { success = false, message = "Không tìm thấy từ vựng hoặc bạn không có quyền truy cập." });
                }

                // Kiểm tra nếu đổi tên từ có trùng với từ khác không
                if (existingItem.Word != vocaItem.Word)
                {
                    var duplicateItem = await _itemRepo.FindByWordAsync(existingItem.VocaSetId, vocaItem.Word);
                    if (duplicateItem != null && duplicateItem.Id != vocaItem.Id)
                    {
                        return Json(new { success = false, message = $"Từ vựng '{vocaItem.Word}' đã tồn tại trong bộ từ vựng này." });
                    }
                }

                // Giữ nguyên VocaSetId và Status
                vocaItem.VocaSetId = existingItem.VocaSetId;
                vocaItem.Status = existingItem.Status;

                var updatedItem = await _itemRepo.UpdateAsync(vocaItem);
                _logger.LogInformation("User {UserId} updated VocaItem {ItemId}", userId, vocaItem.Id);

                return Json(new { success = true, data = updatedItem, message = "Cập nhật từ vựng thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating VocaItem {ItemId}", vocaItem.Id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật từ vựng." });
            }
        }

        /// <summary>
        /// Xóa từ vựng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var vocaItem = await _itemRepo.GetWithVocaSetAsync(id);

                if (vocaItem == null || vocaItem.VocaSet.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to delete VocaItem {ItemId} without permission", userId, id);
                    return Json(new { success = false, message = "Không tìm thấy từ vựng hoặc bạn không có quyền truy cập." });
                }

                var success = await _itemRepo.DeleteAsync(id);
                if (success)
                {
                    _logger.LogInformation("User {UserId} deleted VocaItem {ItemId}", userId, id);
                    return Json(new { success = true, message = "Xóa từ vựng thành công!" });
                }
                else
                {
                    _logger.LogWarning("Failed to delete VocaItem {ItemId} for User {UserId}", id, userId);
                    return Json(new { success = false, message = "Không thể xóa từ vựng." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting VocaItem {ItemId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa từ vựng." });
            }
        }

        /// <summary>
        /// Import nhiều từ vựng từ bảng dữ liệu (CSV/TSV) vào bộ từ vựng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportItems([FromBody] ImportVocabularyModel model)
        {
            try
            {
                if (model == null || model.VocaSetId <= 0 || model.Items == null || !model.Items.Any())
                {
                    return Json(new { success = false, message = "Dữ liệu import không hợp lệ." });
                }

                // Kiểm tra quyền sở hữu bộ từ vựng
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var vocaSet = await _setRepo.GetByIdAsync(model.VocaSetId);

                if (vocaSet == null || vocaSet.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to import items to VocaSet {VocaSetId} without permission", userId, model.VocaSetId);
                    return Json(new { success = false, message = "Không tìm thấy bộ từ vựng hoặc bạn không có quyền truy cập." });
                }

                // Lấy danh sách từ vựng hiện có để kiểm tra trùng lặp
                var existingItems = await _itemRepo.GetByVocaSetIdAsync(model.VocaSetId);
                var existingWords = existingItems.Select(i => i.Word.ToLower()).ToHashSet();

                // Lọc ra các từ vựng không trùng lặp
                var newItems = new List<VocaItem>();
                var duplicates = new List<string>();

                foreach (var item in model.Items)
                {
                    if (string.IsNullOrWhiteSpace(item.Word))
                    {
                        continue; // Bỏ qua các từ trống
                    }

                    if (existingWords.Contains(item.Word.ToLower()))
                    {
                        duplicates.Add(item.Word);
                        continue;
                    }

                    item.VocaSetId = model.VocaSetId;
                    item.Status = "notlearned";
                    newItems.Add(item);
                    existingWords.Add(item.Word.ToLower()); // Thêm vào hash set để tránh trùng lặp trong batch import
                }

                // Thêm các từ vựng mới
                if (newItems.Any())
                {
                    await _itemRepo.AddRangeAsync(newItems);
                    _logger.LogInformation("User {UserId} imported {Count} items to VocaSet {VocaSetId}", userId, newItems.Count, model.VocaSetId);
                }

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        total = model.Items.Count,
                        imported = newItems.Count,
                        duplicates = duplicates.Count,
                        duplicateWords = duplicates
                    },
                    message = $"Đã import {newItems.Count} từ vựng thành công. {duplicates.Count} từ bị bỏ qua do trùng lặp."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing items to VocaSet {VocaSetId}", model?.VocaSetId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi import từ vựng." });
            }
        }

        #endregion

        #region Flashcard & Learning Features

        /// <summary>
        /// Cập nhật trạng thái học tập của một từ vựng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateLearningStatus(int id, string status)
        {
            try
            {
                if (string.IsNullOrEmpty(status) || !new[] { "learned", "notlearned", "reviewing" }.Contains(status))
                {
                    return Json(new { success = false, message = "Trạng thái không hợp lệ." });
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var vocaItem = await _itemRepo.GetWithVocaSetAsync(id);

                if (vocaItem == null)
                {
                    _logger.LogWarning("User {UserId} attempted to update status for non-existent VocaItem {ItemId}", userId, id);
                    return Json(new { success = false, message = "Không tìm thấy từ vựng." });
                }

                // Cập nhật trạng thái
                var result = await _itemRepo.UpdateLearningStatusAsync(id, status);
                if (result)
                {
                    // Tạo lịch sử học tập
                    await _historyRepo.UpdateLearningStatusAsync(userId, id, status);
                    _logger.LogInformation("User {UserId} updated learning status for VocaItem {ItemId} to {Status}", userId, id, status);
                    return Json(new { success = true, message = "Cập nhật trạng thái học tập thành công!" });
                }
                else
                {
                    _logger.LogWarning("Failed to update learning status for VocaItem {ItemId}", id);
                    return Json(new { success = false, message = "Không thể cập nhật trạng thái học tập." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating learning status for VocaItem {ItemId}", id);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái học tập." });
            }
        }

        /// <summary>
        /// Phản hồi kết quả học tập từ trang Flashcard
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int id, bool known)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var vocaItem = await _itemRepo.GetWithVocaSetAsync(id);

                if (vocaItem == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy từ vựng.";
                    return RedirectToAction("Index", "VocaSet");
                }

                // Cập nhật trạng thái học tập
                var status = known ? "learned" : "notlearned";
                await _itemRepo.UpdateLearningStatusAsync(id, status);

                // Tạo lịch sử học tập
                await _historyRepo.UpdateLearningStatusAsync(userId, id, status);

                _logger.LogInformation("User {UserId} reviewed VocaItem {ItemId} as {Status}", userId, id, status);

                // Chuyển hướng đến trang Flashcard để tiếp tục học
                return RedirectToAction("Flashcard", "VocaSet", new { id = vocaItem.VocaSetId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing review for VocaItem {ItemId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật kết quả học tập.";
                return RedirectToAction("Index", "VocaSet");
            }
        }

        #endregion

        #region Models hỗ trợ

        /// <summary>
        /// Model phục vụ cho tính năng import nhiều từ vựng
        /// </summary>
        public class ImportVocabularyModel
        {
            public int VocaSetId { get; set; }
            public List<VocaItem> Items { get; set; } = new();
        }

        #endregion
    }
}