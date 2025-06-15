using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VocaWebApp.Data;
using VocaWebApp.Models;
using VocaWebApp.Data.Repositories;

namespace VocaWebApp.Controllers
{
    /// <summary>
    /// Controller xử lý các chức năng Dashboard - Bảng điều khiển người dùng
    /// Hiển thị thống kê, tiến độ học tập và các thông tin tổng quan
    /// </summary>
    [Authorize]
    [Route("Dashboard")]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IVocaSetRepository _vocaSetRepository;
        private readonly IVocaItemRepository _vocaItemRepository;
        private readonly IVocaItemHistoryRepository _historyRepository;
        private readonly IReviewReminderRepository _reminderRepository;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IVocaSetRepository vocaSetRepository,
            IVocaItemRepository vocaItemRepository,
            IVocaItemHistoryRepository historyRepository,
            IReviewReminderRepository reminderRepository,
            ILogger<DashboardController> logger)
        {
            _userManager = userManager;
            _context = context;
            _vocaSetRepository = vocaSetRepository;
            _vocaItemRepository = vocaItemRepository;
            _historyRepository = historyRepository;
            _reminderRepository = reminderRepository;
            _logger = logger;
        }

        /// <summary>
        /// Trang Dashboard chính - Hiển thị tổng quan thống kê học tập
        /// Bao gồm: số từ đã thuộc, số từ học hôm nay, bộ từ vựng gần đây, gợi ý ôn tập
        /// </summary>
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("User not found in Dashboard/Index");
                    return RedirectToAction("Login", "Account");
                }

                var userId = user.Id;

                // Lấy UserSettings hoặc tạo mặc định
                var userSettings = await _context.UserSettings
                    .FirstOrDefaultAsync(s => s.UserId == userId);

                if (userSettings == null)
                {
                    userSettings = new UserSettings
                    {
                        UserId = userId,
                        DailyGoal = 10,
                        EmailNotifications = true,
                        WebNotifications = true,
                        PreferredLanguage = "vi",
                        Theme = "light",
                        AutoPlayAudio = false,
                        DefaultReviewInterval = 7,
                        FlashcardSessionSize = 20,
                        ShowPronunciation = true,
                        ShowWordType = true,
                        DefaultFlashcardMode = "word-to-meaning",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserSettings.Add(userSettings);
                    await _context.SaveChangesAsync();
                }

                // Thống kê tổng số từ đã thuộc
                var totalLearnedWords = await _historyRepository.CountLearnedWordsAsync(userId);

                // Thống kê số từ thuộc hôm nay
                var todayLearnedWords = await _historyRepository.CountTodayLearnedWordsAsync(userId);

                // Lấy danh sách bộ từ vựng truy cập gần đây (5 bộ gần nhất)
                var recentVocaSets = await _vocaSetRepository.GetRecentlyAccessedAsync(userId, 5);

                // Lấy các gợi ý ôn tập (ReviewReminder sắp đến hạn)
                var upcomingReminders = await _reminderRepository.GetByUserIdAsync(userId);
                var pendingReviews = upcomingReminders
                    .Where(r => !r.IsSent && r.ReviewDate <= DateTime.UtcNow.AddDays(1))
                    .OrderBy(r => r.ReviewDate)
                    .Take(5)
                    .ToList();

                // Thống kê tiến độ học tập cá nhân
                var userVocaSets = await _vocaSetRepository.GetByUserIdAsync(userId);
                var totalVocaSets = userVocaSets.Count();
                var publicVocaSets = userVocaSets.Count(v => v.Status != "private");
                var privateVocaSets = userVocaSets.Count(v => v.Status == "private");

                // Tính tổng số từ vựng
                var totalWords = 0;
                foreach (var vocaSet in userVocaSets)
                {
                    totalWords += await _vocaItemRepository.GetCountByVocaSetIdAsync(vocaSet.Id);
                }

                // Tính tiến độ đạt mục tiêu hàng ngày
                var dailyGoal = userSettings.DailyGoal;
                var dailyProgress = dailyGoal > 0 ? Math.Min((double)todayLearnedWords / dailyGoal * 100, 100) : 0;

                // Thống kê xu hướng học tập 7 ngày gần đây
                var weeklyStats = new Dictionary<DateTime, int>();
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.Date.AddDays(-i);
                    var nextDate = date.AddDays(1);

                    var wordsLearnedOnDate = await _context.VocaItemHistories
                        .Where(h => h.UserId == userId &&
                                   h.ReviewedAt >= date &&
                                   h.ReviewedAt < nextDate &&
                                   h.Status == "learned")
                        .Select(h => h.VocaItemId)
                        .Distinct()
                        .CountAsync();

                    weeklyStats[date] = wordsLearnedOnDate;
                }

                // Tạo ViewModel
                var viewModel = new DashboardViewModel
                {
                    // Thông tin user
                    UserFullName = user.FullName,
                    UserProfileImageUrl = user.ProfileImageUrl,

                    // Thống kê chính
                    TotalLearnedWords = totalLearnedWords,
                    TodayLearnedWords = todayLearnedWords,
                    DailyGoal = dailyGoal,
                    DailyProgress = dailyProgress,

                    // Thống kê bộ từ vựng
                    TotalVocaSets = totalVocaSets,
                    PublicVocaSets = publicVocaSets,
                    PrivateVocaSets = privateVocaSets,
                    TotalWords = totalWords,

                    // Danh sách
                    RecentVocaSets = recentVocaSets.ToList(),
                    PendingReviews = pendingReviews,

                    // Thống kê xu hướng
                    WeeklyStats = weeklyStats,

                    // Settings
                    UserSettings = userSettings
                };

                _logger.LogInformation($"Dashboard loaded for user {userId}");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải dashboard.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Cập nhật mục tiêu học tập hàng ngày (Daily Goal)
        /// </summary>
        [HttpPost]
        [Route("UpdateDailyGoal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDailyGoal(int dailyGoal)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                }

                if (dailyGoal < 1 || dailyGoal > 100)
                {
                    return Json(new { success = false, message = "Mục tiêu hàng ngày phải từ 1 đến 100 từ." });
                }

                var userSettings = await _context.UserSettings
                    .FirstOrDefaultAsync(s => s.UserId == user.Id);

                if (userSettings == null)
                {
                    userSettings = new UserSettings
                    {
                        UserId = user.Id,
                        DailyGoal = dailyGoal,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.UserSettings.Add(userSettings);
                }
                else
                {
                    userSettings.DailyGoal = dailyGoal;
                    userSettings.UpdatedAt = DateTime.UtcNow;
                    _context.UserSettings.Update(userSettings);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"User {user.Id} updated daily goal to {dailyGoal}");
                return Json(new { success = true, message = "Mục tiêu hàng ngày đã được cập nhật." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating daily goal");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật mục tiêu." });
            }
        }

        /// <summary>
        /// Lấy dữ liệu thống kê để hiển thị biểu đồ Ajax
        /// </summary>
        [HttpGet]
        [Route("GetWeeklyStats")]
        public async Task<IActionResult> GetWeeklyStats()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                }

                var weeklyStats = new List<object>();
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.Date.AddDays(-i);
                    var nextDate = date.AddDays(1);

                    var wordsLearned = await _context.VocaItemHistories
                        .Where(h => h.UserId == user.Id &&
                                   h.ReviewedAt >= date &&
                                   h.ReviewedAt < nextDate &&
                                   h.Status == "learned")
                        .Select(h => h.VocaItemId)
                        .Distinct()
                        .CountAsync();

                    weeklyStats.Add(new
                    {
                        date = date.ToString("MM/dd"),
                        words = wordsLearned
                    });
                }

                return Json(new { success = true, data = weeklyStats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekly stats");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê." });
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết thống kê tổng quan
        /// </summary>
        [HttpGet]
        [Route("GetDetailedStats")]
        public async Task<IActionResult> GetDetailedStats()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dụng." });
                }

                var userId = user.Id;

                // Thống kê theo trạng thái học tập
                var learningStats = await _historyRepository.GetOverallLearningStatisticsAsync(userId);

                // Thống kê theo thời gian (30 ngày gần đây)
                var monthlyStats = new List<object>();
                for (int i = 29; i >= 0; i--)
                {
                    var date = DateTime.UtcNow.Date.AddDays(-i);
                    var nextDate = date.AddDays(1);

                    var wordsLearned = await _context.VocaItemHistories
                        .Where(h => h.UserId == userId &&
                                   h.ReviewedAt >= date &&
                                   h.ReviewedAt < nextDate &&
                                   h.Status == "learned")
                        .Select(h => h.VocaItemId)
                        .Distinct()
                        .CountAsync();

                    monthlyStats.Add(new
                    {
                        date = date.ToString("yyyy-MM-dd"),
                        words = wordsLearned
                    });
                }

                // Thống kê bộ từ vựng phổ biến
                var popularVocaSets = await _context.VocaSets
                    .Where(v => v.UserId == userId && !v.IsDeleted)
                    .OrderByDescending(v => v.ViewCount)
                    .Take(5)
                    .Select(v => new
                    {
                        name = v.Name,
                        views = v.ViewCount,
                        id = v.Id
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    learningStats = learningStats,
                    monthlyStats = monthlyStats,
                    popularVocaSets = popularVocaSets
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed stats");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thống kê chi tiết." });
            }
        }

        /// <summary>
        /// Đánh dấu reminder đã được xem/xử lý
        /// </summary>
        [HttpPost]
        [Route("MarkReminderAsSeen/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkReminderAsSeen(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
                }

                var success = await _reminderRepository.MarkAsSentAsync(id);

                if (success)
                {
                    _logger.LogInformation($"User {user.Id} marked reminder {id} as seen");
                    return Json(new { success = true });
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy reminder." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking reminder {id} as seen");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật reminder." });
            }
        }
    }

    /// <summary>
    /// ViewModel cho trang Dashboard
    /// Chứa tất cả dữ liệu cần thiết hiển thị trên dashboard
    /// </summary>
    public class DashboardViewModel
    {
        // Thông tin người dùng
        public string UserFullName { get; set; } = "";
        public string? UserProfileImageUrl { get; set; }

        // Thống kê học tập chính
        public int TotalLearnedWords { get; set; }
        public int TodayLearnedWords { get; set; }
        public int DailyGoal { get; set; }
        public double DailyProgress { get; set; }

        // Thống kê bộ từ vựng
        public int TotalVocaSets { get; set; }
        public int PublicVocaSets { get; set; }
        public int PrivateVocaSets { get; set; }
        public int TotalWords { get; set; }

        // Danh sách và dữ liệu
        public List<VocaSet> RecentVocaSets { get; set; } = new List<VocaSet>();
        public List<ReviewReminder> PendingReviews { get; set; } = new List<ReviewReminder>();

        // Thống kê xu hướng
        public Dictionary<DateTime, int> WeeklyStats { get; set; } = new Dictionary<DateTime, int>();

        // Cài đặt người dùng
        public UserSettings UserSettings { get; set; } = new UserSettings();
    }
}