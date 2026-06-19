using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tuan6.Models;

namespace Tuan6.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CustomerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CustomerController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public class UserListItem
        {
            public string Id { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string UserName { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
        }

        // GET: /Admin/Customer
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserListItem>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserListItem
                {
                    Id = user.Id,
                    FullName = user.FullName ?? "Chưa cập nhật",
                    Email = user.Email ?? "Chưa cập nhật",
                    UserName = user.UserName ?? "Chưa cập nhật",
                    Role = roles.FirstOrDefault() ?? "Member"
                });
            }

            return View(userList);
        }

        // POST: /Admin/Customer/ToggleRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Prevent self-toggle of role
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == userId)
            {
                TempData["ErrorMessage"] = "Bạn không thể tự thay đổi vai trò của chính mình!";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var currentRole = roles.FirstOrDefault() ?? "Member";
            
            string newRole = currentRole == "Admin" ? "Member" : "Admin";

            // Remove existing roles
            await _userManager.RemoveFromRolesAsync(user, roles);
            
            // Add new role
            var result = await _userManager.AddToRoleAsync(user, newRole);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Đã đổi vai trò của {user.FullName} sang {newRole}!";
            }
            else
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đổi vai trò.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Customer/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Role = roles.FirstOrDefault() ?? "Member";

            // Load order history
            var orders = await _context.Orders
                .Where(o => o.UserId == id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.Orders = orders;

            return View(user);
        }
    }
}
