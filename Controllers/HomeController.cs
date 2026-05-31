using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tuan6.Models;
using Tuan6.Repositories;

namespace Tuan6.Controllers;

public class HomeController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ILogger<HomeController> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<IActionResult> Index(int? categoryId)
    {
        var products = await _productRepository.GetAllAsync();
        var categories = await _categoryRepository.GetAllAsync();

        if (categoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == categoryId.Value).ToList();
        }

        ViewBag.Categories = categories;
        ViewBag.SelectedCategoryId = categoryId;
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
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
