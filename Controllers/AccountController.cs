using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tuan6.Models;
using Tuan6.Services;

namespace Tuan6.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FullName = model.FullName,
                    Address = model.Address
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Check and create Member role if not exists
                    if (!await _roleManager.RoleExistsAsync("Member"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Member"));
                    }

                    // Assign "Member" role by default
                    await _userManager.AddToRoleAsync(user, "Member");

                    // Sign the user in
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    Tuan6.Helpers.SessionExtensions.MergeCart(HttpContext.Session, user.Id);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                // Try finding by UserName or Email
                var user = await _userManager.FindByNameAsync(model.UserNameOrEmail) 
                           ?? await _userManager.FindByEmailAsync(model.UserNameOrEmail);

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        Tuan6.Helpers.SessionExtensions.MergeCart(HttpContext.Session, user.Id);
                        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        {
                            return Redirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Tên đăng nhập/Email hoặc mật khẩu không chính xác.");
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Session.Clear(); // Clear session on logout
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/Profile
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName ?? string.Empty,
                Address = user.Address ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Email = user.Email ?? string.Empty
            };

            return View(model);
        }

        // POST: /Account/Profile
        [HttpPost]
        [Authorize]
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
                return RedirectToAction("Login");
            }

            // Verify email isn't taken by another user
            if (user.Email != model.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng bởi tài khoản khác.");
                    return View(model);
                }
            }

            user.FullName = model.FullName;
            user.Address = model.Address;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.UserName = model.Email; // Keep UserName and Email synced if you use Email for login, or keep UserName as is

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Refresh sign-in cookie to reflect new username/claims
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công.";
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email này không tồn tại trong hệ thống.");
                return View(model);
            }

            // Generate 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Store in Session
            HttpContext.Session.SetString("PasswordResetEmail", model.Email);
            HttpContext.Session.SetString("PasswordResetOtp", otp);
            HttpContext.Session.SetString("PasswordResetOtpExpiry", DateTime.Now.AddMinutes(5).ToString());

            // Send via email service
            var subject = "Mã xác thực khôi phục mật khẩu";
            var body = $"<h3>Mã xác thực (OTP) của bạn là: <b style='color:#5e72e4;font-size:24px;letter-spacing:2px;'>{otp}</b></h3><p>Mã này có hiệu lực trong vòng 5 phút.</p>";
            await _emailService.SendEmailAsync(model.Email, subject, body);

            // Save to TempData for simulation display
            TempData["SimulatedOtp"] = otp;

            return RedirectToAction("VerifyOtp", new { email = model.Email });
        }

        // GET: /Account/VerifyOtp
        [HttpGet]
        public IActionResult VerifyOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            // Keep simulated OTP if redirecting
            TempData.Keep("SimulatedOtp");

            var model = new VerifyOtpViewModel { Email = email };
            return View(model);
        }

        // POST: /Account/VerifyOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var sessionEmail = HttpContext.Session.GetString("PasswordResetEmail");
            var sessionOtp = HttpContext.Session.GetString("PasswordResetOtp");
            var sessionExpiryStr = HttpContext.Session.GetString("PasswordResetOtpExpiry");

            if (string.IsNullOrEmpty(sessionEmail) || string.IsNullOrEmpty(sessionOtp) || string.IsNullOrEmpty(sessionExpiryStr))
            {
                ModelState.AddModelError(string.Empty, "Yêu cầu khôi phục mật khẩu không hợp lệ hoặc đã quá hạn.");
                return View(model);
            }

            if (sessionEmail != model.Email || sessionOtp != model.OtpCode)
            {
                TempData.Keep("SimulatedOtp");
                ModelState.AddModelError(string.Empty, "Mã xác nhận (OTP) không chính xác.");
                return View(model);
            }

            if (DateTime.TryParse(sessionExpiryStr, out var expiry) && DateTime.Now > expiry)
            {
                TempData.Keep("SimulatedOtp");
                ModelState.AddModelError(string.Empty, "Mã xác nhận (OTP) đã hết hạn.");
                return View(model);
            }

            // Verification successful! Generate password reset token
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra. Không tìm thấy tài khoản.");
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Store verification success flag in session
            HttpContext.Session.SetString("OtpVerified", "true");

            return RedirectToAction(nameof(ResetPassword), new { email = model.Email, token = token });
        }

        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            ViewBag.ResetLink = TempData["ResetLink"];
            return View();
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (email == null || token == null)
            {
                return BadRequest("Email and token are required for password reset.");
            }

            // Verify OTP verification was successful
            var otpVerified = HttpContext.Session.GetString("OtpVerified");
            var sessionEmail = HttpContext.Session.GetString("PasswordResetEmail");
            if (otpVerified != "true" || sessionEmail != email)
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Verify OTP verification was successful
            var otpVerified = HttpContext.Session.GetString("OtpVerified");
            var sessionEmail = HttpContext.Session.GetString("PasswordResetEmail");
            if (otpVerified != "true" || sessionEmail != model.Email)
            {
                return RedirectToAction(nameof(ForgotPassword));
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                // Clear session
                HttpContext.Session.Remove("PasswordResetEmail");
                HttpContext.Session.Remove("PasswordResetOtp");
                HttpContext.Session.Remove("PasswordResetOtpExpiry");
                HttpContext.Session.Remove("OtpVerified");

                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi từ dịch vụ ngoài: {remoteError}");
                return View("Login");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Không lấy được thông tin đăng nhập Google.");
                return View("Login");
            }

            // Sign in the user with this external login provider if they already have a login
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                if (user != null)
                {
                    Tuan6.Helpers.SessionExtensions.MergeCart(HttpContext.Session, user.Id);
                }
                return LocalRedirect(returnUrl);
            }

            // Get the email from Google info
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email != null)
            {
                // Find if user already exists
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Create a new user
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email,
                        Address = string.Empty
                    };
                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        foreach (var error in createResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View("Login");
                    }

                    // Assign default Member role
                    if (!await _roleManager.RoleExistsAsync("Member"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Member"));
                    }
                    await _userManager.AddToRoleAsync(user, "Member");
                }

                // Link external login to user
                var addLoginResult = await _userManager.AddLoginAsync(user, info);
                if (addLoginResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                    Tuan6.Helpers.SessionExtensions.MergeCart(HttpContext.Session, user.Id);
                    return LocalRedirect(returnUrl);
                }
            }

            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi đăng nhập bằng Google.");
            return View("Login");
        }
    }
}
