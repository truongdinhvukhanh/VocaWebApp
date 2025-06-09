using Microsoft.AspNetCore.Mvc;
using VocaWebApp.Models;
using VocaWebApp.Repositories;
using System.Threading.Tasks;

namespace VocaWebApp.Controllers
{
    [Route("vocaitems")]
    public class VocaItemController : Controller
    {
        private readonly IVocaItemRepository _vocaItemRepository;

        public VocaItemController(IVocaItemRepository vocaItemRepository)
        {
            _vocaItemRepository = vocaItemRepository;
        }

        // AJAX: Lấy danh sách VocaItem theo SetId (trả về PartialView hoặc JSON)
        [HttpGet("ajax/byset/{setId}")]
        public async Task<IActionResult> GetBySetAjax(int setId)
        {
            var items = await _vocaItemRepository.GetBySetIdAsync(setId);
            // return PartialView("_VocaItemListPartial", items); // Nếu dùng PartialView
            return Json(items); // Nếu dùng AJAX JSON
        }

        // AJAX: Thêm mới VocaItem (trả về JSON hoặc PartialView)
        [HttpPost("ajax/create")]
        public async Task<IActionResult> CreateAjax([FromBody] VocaItem item)
        {
            if (ModelState.IsValid)
            {
                var newItem = await _vocaItemRepository.AddAsync(item);
                return Json(new { success = true, item = newItem });
            }
            return Json(new { success = false, errors = ModelState.Values });
        }

        // AJAX: Sửa VocaItem (trả về JSON hoặc PartialView)
        [HttpPost("ajax/edit")]
        public async Task<IActionResult> EditAjax([FromBody] VocaItem item)
        {
            if (ModelState.IsValid)
            {
                var updatedItem = await _vocaItemRepository.UpdateAsync(item);
                return Json(new { success = true, item = updatedItem });
            }
            return Json(new { success = false, errors = ModelState.Values });
        }

        // AJAX: Xóa VocaItem (trả về JSON)
        [HttpPost("ajax/delete")]
        public async Task<IActionResult> DeleteAjax([FromBody] int id)
        {
            var success = await _vocaItemRepository.DeleteAsync(id);
            return Json(new { success });
        }

        // Các action cũ vẫn giữ lại nếu muốn dùng view truyền thống

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _vocaItemRepository.GetByIdAsync(id);
            if (item == null)
                return NotFound();
            return View(item);
        }

        [HttpGet("set/{setId}")]
        public async Task<IActionResult> BySet(int setId)
        {
            var items = await _vocaItemRepository.GetBySetIdAsync(setId);
            return View(items);
        }

        [HttpGet("create")]
        public IActionResult Create() => View();

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VocaItem item)
        {
            if (ModelState.IsValid)
            {
                await _vocaItemRepository.AddAsync(item);
                return RedirectToAction(nameof(BySet), new { setId = item.VocaSetId });
            }
            return View(item);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _vocaItemRepository.GetByIdAsync(id);
            if (item == null)
                return NotFound();
            return View(item);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VocaItem item)
        {
            if (id != item.Id)
                return BadRequest();

            if (ModelState.IsValid)
            {
                await _vocaItemRepository.UpdateAsync(item);
                return RedirectToAction(nameof(BySet), new { setId = item.VocaSetId });
            }
            return View(item);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _vocaItemRepository.GetByIdAsync(id);
            if (item == null)
                return NotFound();
            return View(item);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _vocaItemRepository.GetByIdAsync(id);
            if (item != null)
                await _vocaItemRepository.DeleteAsync(id);
            return RedirectToAction(nameof(BySet), new { setId = item?.VocaSetId });
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> ByUser(string userId)
        {
            var items = await _vocaItemRepository.GetByUserAsync(userId);
            return View(items);
        }

        [HttpPost("{id}/status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            await _vocaItemRepository.UpdateStatusAsync(id, status);
            var item = await _vocaItemRepository.GetByIdAsync(id);
            return RedirectToAction(nameof(BySet), new { setId = item?.VocaSetId });
        }
    }
}