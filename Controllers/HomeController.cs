using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VocaWebApp.Data.Repositories;
using VocaWebApp.Models;

namespace VocaWebApp.Controllers
{
    /// <summary>
    /// Controller xử lý trang chủ và các tính năng chính
    /// Hiển thị thông tin giới thiệu, liên kết nhanh đến các tính năng
    /// Tùy chọn tìm kiếm bộ từ vựng nếu có
    /// </summary>
    public class HomeController : Controller
    {
        private readonly IVocaSetRepository _vocaSetRepository;
        private readonly IVocaItemRepository _vocaItemRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            IVocaSetRepository vocaSetRepository,
            IVocaItemRepository vocaItemRepository,
            UserManager<ApplicationUser> userManager)
        {
            _vocaSetRepository = vocaSetRepository;
            _vocaItemRepository = vocaItemRepository;
            _userManager = userManager;
        }

        /// <summary>
        /// Trang chủ - Hiển thị thông tin giới thiệu ứng dụng
        /// Liên kết nhanh đến các tính năng chính: đăng nhập, đăng ký, khám phá bộ từ vựng
        /// Tùy chọn tìm kiếm bộ từ vựng nếu có
        /// </summary>
        public async Task<IActionResult> Index(string? searchKeyword = null)
        {
            var model = new HomeViewModel();

            // Thống kê tổng quan hệ thống
            var allVocaSets = await _vocaSetRepository.GetAllAsync();
            var publicVocaSets = await _vocaSetRepository.GetPublicViewableAsync();

            model.TotalVocaSets = allVocaSets.Count();
            model.PublicVocaSets = publicVocaSets.Count();

            // Tính tổng số từ vựng trong hệ thống
            var totalWords = 0;
            foreach (var vocaSet in allVocaSets)
            {
                totalWords += await _vocaItemRepository.GetCountByVocaSetIdAsync(vocaSet.Id);
            }
            model.TotalWords = totalWords;

            // Bộ từ vựng phổ biến nhất (top 6 để hiển thị trên trang chủ)
            model.PopularVocaSets = await _vocaSetRepository.GetMostPopularAsync(6);

            // Tìm kiếm bộ từ vựng nếu có từ khóa
            if (!string.IsNullOrWhiteSpace(searchKeyword))
            {
                model.SearchResults = await _vocaSetRepository.SearchAsync(searchKeyword, null, true);
                model.SearchKeyword = searchKeyword;
            }

            return View(model);
        }

        /// <summary>
        /// Trang giới thiệu về ứng dụng
        /// </summary>
        public IActionResult About()
        {
            return View();
        }

        /// <summary>
        /// Trang liên hệ
        /// </summary>
        public IActionResult Contact()
        {
            return View();
        }

        /// <summary>
        /// Trang chính sách bảo mật
        /// </summary>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Trang điều khoản sử dụng
        /// </summary>
        public IActionResult Terms()
        {
            return View();
        }

        /// <summary>
        /// Xử lý lỗi
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        /// <summary>
        /// API endpoint cho tìm kiếm nhanh bộ từ vựng (sử dụng AJAX)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> QuickSearch(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Json(new { success = false, message = "Từ khóa tìm kiếm không được để trống" });
            }

            try
            {
                var results = await _vocaSetRepository.SearchAsync(keyword, null, true);
                var searchResults = results.Take(5).Select(v => new
                {
                    id = v.Id,
                    name = v.Name,
                    description = v.Description,
                    wordCount = v.VocaItems?.Count ?? 0,
                    viewCount = v.ViewCount,
                    createdBy = v.User?.FullName ?? "Ẩn danh"
                });

                return Json(new { success = true, data = searchResults });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra khi tìm kiếm" });
            }
        }

        /// <summary>
        /// Hiển thị thống kê hệ thống cho admin
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SystemStats()
        {
            if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var stats = new SystemStatsViewModel();

            var allVocaSets = await _vocaSetRepository.GetAllAsync();
            stats.TotalVocaSets = allVocaSets.Count();
            stats.PublicVocaSets = allVocaSets.Count(v => v.Status == "public-view" || v.Status == "public-copy");
            stats.PrivateVocaSets = allVocaSets.Count(v => v.Status == "private");

            // Tính tổng số từ vựng
            var totalWords = 0;
            foreach (var vocaSet in allVocaSets)
            {
                totalWords += await _vocaItemRepository.GetCountByVocaSetIdAsync(vocaSet.Id);
            }
            stats.TotalWords = totalWords;

            // Tổng số người dùng
            stats.TotalUsers = _userManager.Users.Count();
            stats.ActiveUsers = _userManager.Users.Count(u => u.IsActive);

            return Json(stats);
        }
    }

    /// <summary>
    /// ViewModel cho trang chủ
    /// </summary>
    public class HomeViewModel
    {
        public int TotalVocaSets { get; set; }
        public int PublicVocaSets { get; set; }
        public int TotalWords { get; set; }
        public IEnumerable<VocaSet> PopularVocaSets { get; set; } = new List<VocaSet>();
        public IEnumerable<VocaSet> SearchResults { get; set; } = new List<VocaSet>();
        public string? SearchKeyword { get; set; }
    }

    /// <summary>
    /// ViewModel cho thống kê hệ thống
    /// </summary>
    public class SystemStatsViewModel
    {
        public int TotalVocaSets { get; set; }
        public int PublicVocaSets { get; set; }
        public int PrivateVocaSets { get; set; }
        public int TotalWords { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
    }

    /// <summary>
    /// ViewModel cho xử lý lỗi
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}