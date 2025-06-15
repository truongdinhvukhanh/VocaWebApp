using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Authentication;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Identity;

using Microsoft.AspNetCore.Mvc;

using VocaWebApp.Models;

using VocaWebApp.Models.AccountViewModels;

namespace VocaWebApp.Controllers
{
    /// <summary>
    /// Controller xử lý các chức năng xác thực: đăng ký, đăng nhập, đăng xuất
    /// Sử dụng Identity để quản lý user authentication
    /// </summary>
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị trang đăng ký
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Xử lý đăng ký user mới
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Tạo ApplicationUser từ thông tin đăng ký
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = model.FullName,
                    // Tự động gán avatar mặc định
                    ProfileImageUrl = "/images/default-avatar.png",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                // Tạo user với Identity
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    // Tự động đăng nhập sau khi đăng ký thành công
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation("User logged in after registration.");

                    // Cập nhật thời gian đăng nhập
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userManager.UpdateAsync(user);

                    // Chuyển về trang chủ hoặc returnUrl
                    return RedirectToLocal(returnUrl);
                }

                // Hiển thị lỗi nếu đăng ký không thành công
                AddErrors(result);
            }

            return View(model);
        }

        /// <summary>
        /// Hiển thị trang đăng nhập
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Xóa external cookie để đảm bảo clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Thử đăng nhập với email và password
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    // Cập nhật thời gian đăng nhập cuối
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        user.LastLoginAt = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                    }

                    return RedirectToLocal(returnUrl);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa tạm thời.");
                    return View(model);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                    return View(model);
                }
            }

            return View(model);
        }

        /// <summary>
        /// Xử lý đăng xuất
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        /// <summary>
        /// Trang Access Denied
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
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
                { "Passwords must have at least one digit ('0'-'9').", "Mật khẩu phải có ít nhất một chữ số." },
                { "Passwords must have at least one uppercase ('A'-'Z').", "Mật khẩu phải có ít nhất một chữ hoa." },
                { "Passwords must have at least one lowercase ('a'-'z').", "Mật khẩu phải có ít nhất một chữ thường." },
                { "Passwords must have at least one non alphanumeric character.", "Mật khẩu phải có ít nhất một ký tự đặc biệt." },
                { "Email is already taken.", "Email này đã được sử dụng." },
                { "User name is already taken.", "Tên đăng nhập đã được sử dụng." }
            };

            return errorTranslations.ContainsKey(error) ? errorTranslations[error] : error;
        }

        /// <summary>
        /// Redirect về local URL an toàn
        /// </summary>
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}