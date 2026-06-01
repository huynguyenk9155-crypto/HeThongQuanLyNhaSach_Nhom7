using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tuan6.Helpers;
using Tuan6.Models;
using Tuan6.Repositories;

namespace Tuan6.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CartController(
            IBookRepository bookRepository,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _bookRepository = bookRepository;
            _userManager = userManager;
            _context = context;
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObjectAsJson("Cart", cart);
        }

        // GET: /Cart
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sách này.";
                return RedirectToAction("Index", "Home");
            }

            if (book.StockQuantity < quantity)
            {
                TempData["ErrorMessage"] = $"Không đủ hàng tồn kho. Chỉ còn {book.StockQuantity} cuốn.";
                return RedirectToAction("Index", "Home");
            }

            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(i => i.BookId == bookId);

            if (cartItem == null)
            {
                cart.Add(new CartItem
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Price = book.Price,
                    Quantity = quantity,
                    ImageUrl = book.ImageUrl
                });
            }
            else
            {
                if (book.StockQuantity < cartItem.Quantity + quantity)
                {
                    TempData["ErrorMessage"] = $"Không thể thêm. Giỏ hàng có {cartItem.Quantity} cuốn, tồn kho chỉ còn {book.StockQuantity} cuốn.";
                    return RedirectToAction("Index", "Home");
                }
                cartItem.Quantity += quantity;
            }

            SaveCart(cart);
            TempData["SuccessMessage"] = $"Đã thêm '{book.Title}' vào giỏ hàng.";
            return RedirectToAction("Index");
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int bookId, int quantity)
        {
            if (quantity <= 0)
            {
                return RedirectToAction("RemoveFromCart", new { bookId });
            }

            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }

            if (book.StockQuantity < quantity)
            {
                TempData["ErrorMessage"] = $"Không đủ hàng tồn kho. Sách '{book.Title}' chỉ còn {book.StockQuantity} cuốn.";
                return RedirectToAction("Index");
            }

            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(i => i.BookId == bookId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }

        // GET/POST: /Cart/RemoveFromCart
        public IActionResult RemoveFromCart(int bookId)
        {
            var cart = GetCart();
            var cartItem = cart.FirstOrDefault(i => i.BookId == bookId);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                SaveCart(cart);
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }

            return RedirectToAction("Index");
        }

        // POST: /Cart/ClearCart
        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove("Cart");
            TempData["SuccessMessage"] = "Đã xóa toàn bộ giỏ hàng.";
            return RedirectToAction("Index");
        }

        // GET: /Cart/Checkout
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            var order = new Order
            {
                FullName = user?.FullName ?? string.Empty,
                Address = user?.Address ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty,
                TotalAmount = cart.Sum(i => i.TotalPrice)
            };

            return View(order);
        }

        // POST: /Cart/Checkout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order model)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index");
            }

            // Verify stock quantity before checking out
            foreach (var item in cart)
            {
                var book = await _bookRepository.GetByIdAsync(item.BookId);
                if (book == null)
                {
                    ModelState.AddModelError("", $"Sách '{item.Title}' không còn tồn tại trên hệ thống.");
                    return View(model);
                }
                if (book.StockQuantity < item.Quantity)
                {
                    ModelState.AddModelError("", $"Sách '{item.Title}' chỉ còn {book.StockQuantity} cuốn trong kho, không đủ số lượng đặt hàng ({item.Quantity}).");
                    return View(model);
                }
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var order = new Order
                        {
                            UserId = user?.Id,
                            FullName = model.FullName,
                            Address = model.Address,
                            PhoneNumber = model.PhoneNumber,
                            Notes = model.Notes,
                            OrderDate = DateTime.Now,
                            Status = "Pending",
                            TotalAmount = cart.Sum(i => i.TotalPrice)
                        };

                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync(); // Generates order.Id

                        foreach (var item in cart)
                        {
                            var orderDetail = new OrderDetail
                            {
                                OrderId = order.Id,
                                BookId = item.BookId,
                                Quantity = item.Quantity,
                                Price = item.Price
                            };
                            _context.OrderDetails.Add(orderDetail);

                            // Deduct inventory
                            var book = await _context.Books.FindAsync(item.BookId);
                            if (book != null)
                            {
                                book.StockQuantity -= item.Quantity;
                                _context.Entry(book).State = EntityState.Modified;
                            }
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        // Clear cart
                        HttpContext.Session.Remove("Cart");

                        TempData["SuccessMessage"] = "Đặt hàng thành công!";
                        return RedirectToAction("History", "Order");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError("", "Đã có lỗi xảy ra khi lưu đơn hàng. Vui lòng thử lại. " + ex.Message);
                    }
                }
            }

            model.TotalAmount = cart.Sum(i => i.TotalPrice);
            return View(model);
        }
    }
}
