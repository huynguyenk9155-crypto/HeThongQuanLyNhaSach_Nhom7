using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tuan6.Helpers;
using Tuan6.Models;
using Tuan6.Repositories;

namespace Tuan6.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly IBookRepository _bookRepository;

        public WishlistController(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        private string GetWishlistKey()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userId) ? "Wishlist" : $"Wishlist_{userId}";
        }

        private List<WishlistItem> GetWishlist()
        {
            return HttpContext.Session.GetObjectFromJson<List<WishlistItem>>(GetWishlistKey()) ?? new List<WishlistItem>();
        }

        private void SaveWishlist(List<WishlistItem> wishlist)
        {
            HttpContext.Session.SetObjectAsJson(GetWishlistKey(), wishlist);
        }

        // GET: /Wishlist
        public IActionResult Index()
        {
            var wishlist = GetWishlist();
            return View(wishlist);
        }

        // POST: /Wishlist/Toggle
        [HttpPost]
        public async Task<IActionResult> Toggle(int bookId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sách này." });
            }

            var wishlist = GetWishlist();
            var item = wishlist.FirstOrDefault(i => i.BookId == bookId);
            bool added;

            if (item == null)
            {
                wishlist.Add(new WishlistItem
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Price = book.Price,
                    ImageUrl = book.ImageUrl
                });
                added = true;
            }
            else
            {
                wishlist.Remove(item);
                added = false;
            }

            SaveWishlist(wishlist);
            return Json(new { 
                success = true, 
                added = added, 
                count = wishlist.Count, 
                message = added ? $"Đã thêm '{book.Title}' vào danh sách yêu thích." : $"Đã xóa '{book.Title}' khỏi danh sách yêu thích." 
            });
        }

        // POST: /Wishlist/RemoveFromWishlist
        [HttpPost]
        public IActionResult RemoveFromWishlist(int bookId)
        {
            var wishlist = GetWishlist();
            var item = wishlist.FirstOrDefault(i => i.BookId == bookId);
            if (item != null)
            {
                wishlist.Remove(item);
                SaveWishlist(wishlist);
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi danh sách yêu thích.";
            }

            return RedirectToAction("Index");
        }
    }
}
