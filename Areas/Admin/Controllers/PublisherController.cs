using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tuan6.Models;

namespace Tuan6.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PublisherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PublisherController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Publisher
        public async Task<IActionResult> Index()
        {
            var publishers = await _context.Publishers.ToListAsync();
            return View(publishers);
        }

        // GET: /Admin/Publisher/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Admin/Publisher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Publisher publisher)
        {
            if (ModelState.IsValid)
            {
                _context.Add(publisher);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm nhà xuất bản thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(publisher);
        }

        // GET: /Admin/Publisher/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null)
            {
                return NotFound();
            }
            return View(publisher);
        }

        // POST: /Admin/Publisher/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Publisher publisher)
        {
            if (id != publisher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(publisher);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật nhà xuất bản thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublisherExists(publisher.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(publisher);
        }

        // GET: /Admin/Publisher/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var publisher = await _context.Publishers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publisher == null)
            {
                return NotFound();
            }

            return View(publisher);
        }

        // POST: /Admin/Publisher/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher != null)
            {
                // Check if any books are associated with this publisher
                var hasBooks = await _context.Books.AnyAsync(b => b.PublisherId == id);
                if (hasBooks)
                {
                    TempData["ErrorMessage"] = "Không thể xóa nhà xuất bản này vì đã có sách được đăng thuộc nhà xuất bản này!";
                    return RedirectToAction(nameof(Index));
                }

                _context.Publishers.Remove(publisher);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa nhà xuất bản thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PublisherExists(int id)
        {
            return _context.Publishers.Any(e => e.Id == id);
        }
    }
}
