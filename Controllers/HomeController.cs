using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Tuan6.Models;
using Tuan6.Repositories;

namespace Tuan6.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IBookRepository bookRepository,
            ICategoryRepository categoryRepository,
            ILogger<HomeController> logger)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        // GET: /Home/Index
        public async Task<IActionResult> Index(int? categoryId, string? searchString, int pageNumber = 1)
        {
            const int pageSize = 8;
            var books = await _bookRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();

            // Search by Title or Author
            if (!string.IsNullOrEmpty(searchString))
            {
                var query = searchString.ToLower().Trim();
                books = books.Where(b => b.Title.ToLower().Contains(query) || 
                                         b.Author.ToLower().Contains(query)).ToList();
            }

            // Filter by Category
            if (categoryId.HasValue)
            {
                books = books.Where(b => b.CategoryId == categoryId.Value).ToList();
            }

            // Pagination calculation
            var totalBooks = books.Count();
            var totalPages = (int)Math.Ceiling((double)totalBooks / pageSize);
            
            // Boundary checks
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages > 0 ? totalPages : 1));
            
            var paginatedBooks = books
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SearchString = searchString;
            ViewBag.PageNumber = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalBooks = totalBooks;

            return View(paginatedBooks);
        }

        // GET: /Home/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // GET: /Home/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
