using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VocaWebApp.Models;
using VocaWebApp.Models.ManageViewModels;

namespace VocaWebApp.Controllers
{
    /// <summary>
    /// Controller xử lý các chức năng quản lý tài khoản user
    /// Bao gồm: cập nhật thông tin cá nhân, đổi mật khẩu
    /// </summary>
    [Authorize]
    [Route("[controller]/[action]")]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<ManageController> _logger;

        public ManageController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<ManageController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                CurrentProfileImageUrl = user.ProfileImageUrl
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

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                CurrentProfileImageUrl = user.ProfileImageUrl
            };

            return View(model);
        }

        /// <summary>
        /// Xử lý cập nhật thông tin cá nhân
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
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

            // Cập nhật thông tin user
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

            var phoneNumber = user.PhoneNumber;
            if (model.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
                }
            }

            // Cập nhật ảnh đại diện nếu có
            if (!string.IsNullOrEmpty(model.NewProfileImageUrl))
            {
                user.ProfileImageUrl = model.NewProfileImageUrl;
            }

            // Lưu thay đổi
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred updating user with ID '{user.Id}'.");
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Thông tin cá nhân đã được cập nhật thành công.";

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