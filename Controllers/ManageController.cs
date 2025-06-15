using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VocaWebApp.Data;
using VocaWebApp.Models;
using VocaWebApp.Models.ManageViewModels;

namespace VocaWebApp.Controllers
{
    /// <summary>
    /// Controller xử lý các chức năng quản lý tài khoản user
    /// Bao gồm: cập nhật thông tin cá nhân, đổi mật khẩu, cài đặt UserSettings
    /// </summary>
    [Authorize]
    [Route("[controller]/[action]")]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ManageController> _logger;

        public ManageController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            ILogger<ManageController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Trang chính của Manage - hiển thị thông tin tài khoản
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Lấy UserSettings của user
            var userSettings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == user.Id);

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FullName = user.FullName,
                CurrentProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,

                // UserSettings properties với default values
                DailyGoal = userSettings?.DailyGoal ?? 10,
                EmailNotifications = userSettings?.EmailNotifications ?? true,
                WebNotifications = userSettings?.WebNotifications ?? true,
                PreferredLanguage = userSettings?.PreferredLanguage ?? "vi",
                Theme = userSettings?.Theme ?? "light",
                AutoPlayAudio = userSettings?.AutoPlayAudio ?? false,
                DefaultReviewInterval = userSettings?.DefaultReviewInterval ?? 7,
                FlashcardSessionSize = userSettings?.FlashcardSessionSize ?? 20,
                ShowPronunciation = userSettings?.ShowPronunciation ?? true,
                ShowWordType = userSettings?.ShowWordType ?? true,
                DefaultFlashcardMode = userSettings?.DefaultFlashcardMode ?? "word-to-meaning"
            };

            return View(model);
        }

        /// <summary>
        /// Hiển thị trang cập nhật thông tin cá nhân
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Lấy UserSettings của user
            var userSettings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == user.Id);

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FullName = user.FullName,
                CurrentProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,

                // UserSettings properties với default values
                DailyGoal = userSettings?.DailyGoal ?? 10,
                EmailNotifications = userSettings?.EmailNotifications ?? true,
                WebNotifications = userSettings?.WebNotifications ?? true,
                PreferredLanguage = userSettings?.PreferredLanguage ?? "vi",
                Theme = userSettings?.Theme ?? "light",
                AutoPlayAudio = userSettings?.AutoPlayAudio ?? false,
                DefaultReviewInterval = userSettings?.DefaultReviewInterval ?? 7,
                FlashcardSessionSize = userSettings?.FlashcardSessionSize ?? 20,
                ShowPronunciation = userSettings?.ShowPronunciation ?? true,
                ShowWordType = userSettings?.ShowWordType ?? true,
                DefaultFlashcardMode = userSettings?.DefaultFlashcardMode ?? "word-to-meaning"
            };

            return View(model);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin cá nhân và UserSettings
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) });

                foreach (var error in errors)
                {
                    _logger.LogWarning("Validation error - Field: {Field}, Errors: {Errors}",
                        error.Field, string.Join(", ", error.Errors));
                }

                TempData["ErrorMessage"] = "Lỗi validation: " + string.Join(", ",
                    errors.SelectMany(e => e.Errors));
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // Cập nhật thông tin ApplicationUser
            var email = user.Email;
            if (model.Email != email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
                }
            }

            var fullName = user.FullName;
            if (model.FullName != fullName)
            {
                user.FullName = model.FullName;
            }

            // Cập nhật ảnh đại diện nếu có
            if (!string.IsNullOrEmpty(model.NewProfileImageUrl))
            {
                user.ProfileImageUrl = model.NewProfileImageUrl;
            }

            // Lưu thay đổi ApplicationUser
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred updating user with ID '{user.Id}'.");
            }

            // Xử lý UserSettings
            var userSettings = await _context.UserSettings.FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (userSettings == null)
            {
                // Tạo mới UserSettings nếu chưa có
                userSettings = new UserSettings
                {
                    UserId = user.Id,
                    DailyGoal = model.DailyGoal,
                    EmailNotifications = model.EmailNotifications,
                    WebNotifications = model.WebNotifications,
                    PreferredLanguage = model.PreferredLanguage,
                    Theme = model.Theme,
                    AutoPlayAudio = model.AutoPlayAudio,
                    DefaultReviewInterval = model.DefaultReviewInterval,
                    FlashcardSessionSize = model.FlashcardSessionSize,
                    ShowPronunciation = model.ShowPronunciation,
                    ShowWordType = model.ShowWordType,
                    DefaultFlashcardMode = model.DefaultFlashcardMode,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.UserSettings.Add(userSettings);
            }
            else
            {
                // Cập nhật UserSettings hiện có
                userSettings.DailyGoal = model.DailyGoal;
                userSettings.EmailNotifications = model.EmailNotifications;
                userSettings.WebNotifications = model.WebNotifications;
                userSettings.PreferredLanguage = model.PreferredLanguage;
                userSettings.Theme = model.Theme;
                userSettings.AutoPlayAudio = model.AutoPlayAudio;
                userSettings.DefaultReviewInterval = model.DefaultReviewInterval;
                userSettings.FlashcardSessionSize = model.FlashcardSessionSize;
                userSettings.ShowPronunciation = model.ShowPronunciation;
                userSettings.ShowWordType = model.ShowWordType;
                userSettings.DefaultFlashcardMode = model.DefaultFlashcardMode;
                userSettings.UpdatedAt = DateTime.UtcNow;

                _context.UserSettings.Update(userSettings);
            }

            // Lưu thay đổi vào database
            await _context.SaveChangesAsync();

            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Thông tin cá nhân và cài đặt đã được cập nhật thành công.";

            return RedirectToAction(nameof(Profile));
        }

        /// <summary>
        /// Hiển thị trang đổi mật khẩu
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToAction(nameof(SetPassword));
            }

            var model = new ChangePasswordViewModel();
            return View(model);
        }

        /// <summary>
        /// Xử lý đổi mật khẩu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                AddErrors(changePasswordResult);
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            TempData["StatusMessage"] = "Mật khẩu đã được thay đổi thành công.";

            return RedirectToAction(nameof(ChangePassword));
        }

        /// <summary>
        /// Hiển thị trang thiết lập mật khẩu (cho user đăng nhập external)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> SetPassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var hasPassword = await _userManager.HasPasswordAsync(user);

            if (hasPassword)
            {
                return RedirectToAction(nameof(ChangePassword));
            }

            var model = new SetPasswordViewModel();
            return View(model);
        }

        /// <summary>
        /// Xử lý thiết lập mật khẩu
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                AddErrors(addPasswordResult);
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Mật khẩu đã được thiết lập thành công.";

            return RedirectToAction(nameof(SetPassword));
        }

        #region Helpers

        /// <summary>
        /// Thêm lỗi từ IdentityResult vào ModelState
        /// </summary>
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, TranslateError(error.Description));
            }
        }

        /// <summary>
        /// Dịch lỗi Identity sang tiếng Việt
        /// </summary>
        private string TranslateError(string error)
        {
            var errorTranslations = new Dictionary<string, string>
            {
                { "Incorrect password.", "Mật khẩu không chính xác." },
                { "Passwords must have at least one digit ('0'-'9').", "Mật khẩu phải có ít nhất một chữ số." },
                { "Passwords must have at least one uppercase ('A'-'Z').", "Mật khẩu phải có ít nhất một chữ hoa." },
                { "Passwords must have at least one lowercase ('a'-'z').", "Mật khẩu phải có ít nhất một chữ thường." },
                { "Passwords must have at least one non alphanumeric character.", "Mật khẩu phải có ít nhất một ký tự đặc biệt." }
            };

            return errorTranslations.ContainsKey(error) ? errorTranslations[error] : error;
        }

        #endregion
    }

    /// <summary>
    /// ViewModel cho trang thiết lập mật khẩu
    /// </summary>
    public class SetPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; }
    }
}
