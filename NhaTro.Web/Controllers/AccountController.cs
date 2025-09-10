using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using NhaTro.Application.DTOs;
using NhaTro.Application.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using NhaTro.Web.Models;
using Microsoft.AspNetCore.Http; // Thêm dòng này để sử dụng HttpContext.Session

namespace NhaTro.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null) // Nhận returnUrl ở đây
        {
            ViewData["ReturnUrl"] = returnUrl; // Lưu returnUrl vào ViewData để View có thể sử dụng (nếu cần)
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null) // Nhận returnUrl ở đây
        {
            ViewData["ReturnUrl"] = returnUrl; // Lưu returnUrl vào ViewData (nếu cần)

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userService.AuthenticateAsync(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa.");
                return View(model);
            }

            // Lấy tên vai trò từ RoleId
            string roleName = user.RoleId switch
            {
                1 => "Admin",
                2 => "Owner",
                3 => "Tenant",
                _ => "Unknown"
            };

            // Lưu RoleName vào Session (nếu bạn vẫn muốn dùng Session để hiển thị menu trong Layout)
            HttpContext.Session.SetString("RoleName", roleName);

            // Tạo claims cho xác thực
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName), // Hoặc user.Email nếu bạn muốn hiển thị email
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, roleName) // Đặt tên vai trò vào ClaimTypes.Role
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // SignInAsync sẽ tạo cookie xác thực
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // --- PHẦN QUAN TRỌNG: XỬ LÝ CHUYỂN HƯỚNG ---
            // 1. Ưu tiên chuyển hướng đến returnUrl nếu có và hợp lệ
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // 2. Nếu không có returnUrl hoặc không hợp lệ, chuyển hướng dựa trên vai trò
            return roleName switch
            {
                "Admin" => RedirectToAction("Index", "Dashboard", new { area = "Admin" }),
                "Owner" => RedirectToAction("Index", "Home", new { area = "Owner" }),
                "Tenant" => RedirectToAction("Index", "Home", new { area = "Tenant" }),
                _ => RedirectToAction("Index", "Home") // Trang chủ mặc định nếu không khớp vai trò
            };
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu xác nhận không khớp.");
                return View(model);
            }

            try
            {
                var createUserDto = new CreateUserDto
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,
                    Phone = model.Phone,
                    RoleId = model.RoleId,
                    IsActive = true
                };

                await _userService.AddUserAsync(createUserDto);
                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập."; // Thêm thông báo thành công
                return RedirectToAction("Login");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken] // Nên có ValidateAntiForgeryToken cho POST Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear(); // Xóa tất cả dữ liệu Session khi đăng xuất
            // Có thể thêm TempData để hiển thị thông báo đăng xuất thành công
            TempData["Success"] = "Bạn đã đăng xuất thành công.";
            return RedirectToAction("Login", "Account");
        }
    }
}
