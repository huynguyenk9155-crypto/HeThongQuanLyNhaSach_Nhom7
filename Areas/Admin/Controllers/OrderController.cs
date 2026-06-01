using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tuan6.Models;

namespace Tuan6.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Order
        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            
            // Dynamic stats counts
            ViewBag.PendingCount = await _context.Orders.CountAsync(o => o.Status == "Pending");
            ViewBag.ShippingCount = await _context.Orders.CountAsync(o => o.Status == "Shipping");
            ViewBag.CompletedCount = await _context.Orders.CountAsync(o => o.Status == "Completed");
            ViewBag.CancelledCount = await _context.Orders.CountAsync(o => o.Status == "Cancelled");
            
            ViewBag.SelectedStatus = status;

            return View(orders);
        }

        // GET: /Admin/Order/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Book)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: /Admin/Order/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            string[] validStatuses = { "Pending", "Shipping", "Completed", "Cancelled" };
            if (Array.IndexOf(validStatuses, status) > -1)
            {
                order.Status = status;
                _context.Entry(order).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{id} thành '{status}'.";
            }
            else
            {
                TempData["ErrorMessage"] = "Trạng thái không hợp lệ.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
