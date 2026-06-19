using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tuan6.Models;
using Tuan6.Repositories;

namespace Tuan6.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public BookController(
            IBookRepository bookRepository,
            ICategoryRepository categoryRepository,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext context)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        // GET: /Admin/Book
        public async Task<IActionResult> Index()
        {
            var books = await _bookRepository.GetAllAsync();
            return View(books);
        }

        // GET: /Admin/Book/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // GET: /Admin/Book/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            var authors = await _context.Authors.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Authors = new SelectList(authors, "Id", "Name");

            var publishers = await _context.Publishers.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Publishers = new SelectList(publishers, "Id", "Name");

            return View();
        }

        // POST: /Admin/Book/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book, IFormFile? imageFile)
        {
            if (book.AuthorId.HasValue)
            {
                var author = await _context.Authors.FindAsync(book.AuthorId.Value);
                if (author != null)
                {
                    book.Author = author.Name;
                    ModelState.Remove("Author");
                }
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    book.ImageUrl = await SaveImage(imageFile);
                }
                await _bookRepository.AddAsync(book);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", book.CategoryId);

            var authors = await _context.Authors.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Authors = new SelectList(authors, "Id", "Name", book.AuthorId);

            var publishers = await _context.Publishers.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Publishers = new SelectList(publishers, "Id", "Name", book.PublisherId);

            return View(book);
        }

        // GET: /Admin/Book/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", book.CategoryId);

            var authors = await _context.Authors.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Authors = new SelectList(authors, "Id", "Name", book.AuthorId);

            var publishers = await _context.Publishers.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Publishers = new SelectList(publishers, "Id", "Name", book.PublisherId);

            return View(book);
        }

        // POST: /Admin/Book/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book, IFormFile? imageFile)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (book.AuthorId.HasValue)
            {
                var author = await _context.Authors.FindAsync(book.AuthorId.Value);
                if (author != null)
                {
                    book.Author = author.Name;
                    ModelState.Remove("Author");
                }
            }

            if (ModelState.IsValid)
            {
                var dbBook = await _bookRepository.GetByIdAsync(id);
                if (dbBook == null)
                {
                    return NotFound();
                }

                dbBook.Title = book.Title;
                dbBook.Author = book.Author;
                dbBook.AuthorId = book.AuthorId;
                dbBook.PublisherId = book.PublisherId;
                dbBook.Price = book.Price;
                dbBook.Description = book.Description;
                dbBook.StockQuantity = book.StockQuantity;
                dbBook.CategoryId = book.CategoryId;

                if (imageFile != null)
                {
                    dbBook.ImageUrl = await SaveImage(imageFile);
                }

                await _bookRepository.UpdateAsync(dbBook);
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", book.CategoryId);

            var authors = await _context.Authors.OrderBy(a => a.Name).ToListAsync();
            ViewBag.Authors = new SelectList(authors, "Id", "Name", book.AuthorId);

            var publishers = await _context.Publishers.OrderBy(p => p.Name).ToListAsync();
            ViewBag.Publishers = new SelectList(publishers, "Id", "Name", book.PublisherId);

            return View(book);
        }

        // GET: /Admin/Book/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: /Admin/Book/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _bookRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/" + uniqueFileName;
        }
    }
}
