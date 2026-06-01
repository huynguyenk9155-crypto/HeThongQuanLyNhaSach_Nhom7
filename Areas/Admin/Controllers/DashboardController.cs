using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tuan6.Models;

namespace Tuan6.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            // Total books
            var totalBooks = await _context.Books.CountAsync();

            // Total revenue (Only from Completed orders)
            var totalRevenue = await _context.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);

            // Total orders count
            var totalOrders = await _context.Orders.CountAsync();

            // Total users count
            var totalUsers = await _context.Users.CountAsync();

            // Recent orders (Top 5)
            var recentOrders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // Calculate revenue in the last 7 days for the chart
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .Reverse()
                .ToList();

            var weeklyData = new List<decimal>();
            var weeklyLabels = new List<string>();

            foreach (var day in last7Days)
            {
                var startOfDay = day;
                var endOfDay = day.AddDays(1).AddTicks(-1);
                
                var dailyRevenue = await _context.Orders
                    .Where(o => o.Status == "Completed" && o.OrderDate >= startOfDay && o.OrderDate <= endOfDay)
                    .SumAsync(o => o.TotalAmount);

                weeklyData.Add(dailyRevenue);
                
                // Format label as "T2", "T3"... or "Thứ 2", "Thứ 3"... in Vietnamese
                var dayName = day.DayOfWeek switch
                {
                    DayOfWeek.Monday => "T2",
                    DayOfWeek.Tuesday => "T3",
                    DayOfWeek.Wednesday => "T4",
                    DayOfWeek.Thursday => "T5",
                    DayOfWeek.Friday => "T6",
                    DayOfWeek.Saturday => "T7",
                    DayOfWeek.Sunday => "CN",
                    _ => day.DayOfWeek.ToString()
                };
                weeklyLabels.Add(dayName);
            }

            // Calculate category sales percentage
            var completedDetails = await _context.OrderDetails
                .Include(od => od.Book)
                .ThenInclude(b => b!.Category)
                .Where(od => od.Order!.Status == "Completed")
                .ToListAsync();

            var totalBooksSold = completedDetails.Sum(od => od.Quantity);
            var categorySales = completedDetails
                .GroupBy(od => od.Book?.Category?.Name ?? "Khác")
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Count = g.Sum(od => od.Quantity),
                    Percentage = totalBooksSold > 0 ? (double)g.Sum(od => od.Quantity) / totalBooksSold * 100 : 0
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            ViewBag.TotalBooks = totalBooks;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.RecentOrders = recentOrders;
            
            ViewBag.WeeklyLabels = weeklyLabels;
            ViewBag.WeeklyData = weeklyData;
            ViewBag.CategorySales = categorySales;

            return View();
        }
    }
}
