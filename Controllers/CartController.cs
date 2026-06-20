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

        private string GetCartKey()
        {
            return Tuan6.Helpers.SessionExtensions.GetCartKey(User);
        }

        private string GetPromoCodeKey()
        {
            return Tuan6.Helpers.SessionExtensions.GetPromoCodeKey(User);
        }

        private string GetDiscountAmountKey()
        {
            return Tuan6.Helpers.SessionExtensions.GetDiscountAmountKey(User);
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItem>>(GetCartKey()) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObjectAsJson(GetCartKey(), cart);
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

        // POST: /Cart/AddToCartJson
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AddToCartJson(int bookId, int quantity = 1)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sách này." });
            }

            if (book.StockQuantity < quantity)
            {
                return Json(new { success = false, message = $"Không đủ hàng tồn kho. Chỉ còn {book.StockQuantity} cuốn." });
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
                    return Json(new { success = false, message = $"Không thể thêm. Giỏ hàng có {cartItem.Quantity} cuốn, tồn kho chỉ còn {book.StockQuantity} cuốn." });
                }
                cartItem.Quantity += quantity;
            }

            SaveCart(cart);
            return Json(new { success = true, message = $"Đã thêm '{book.Title}' vào giỏ hàng.", cartCount = cart.Sum(i => i.Quantity) });
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

        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(GetCartKey());
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
            
            // Read promo from session
            decimal discount = 0;
            var discountStr = HttpContext.Session.GetString(GetDiscountAmountKey());
            if (!string.IsNullOrEmpty(discountStr) && decimal.TryParse(discountStr, out var d))
            {
                discount = d;
            }
            
            var totalAmount = cart.Sum(i => i.TotalPrice) - discount;
            if (totalAmount < 0) totalAmount = 0;

            var order = new Order
            {
                FullName = user?.FullName ?? string.Empty,
                Address = user?.Address ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty,
                TotalAmount = totalAmount
            };

            return View(order);
        }

        // POST: /Cart/Checkout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order model, string paymentMethod = "cod", string orderCode = "")
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

            ModelState.Remove("Status");
            ModelState.Remove("User");
            ModelState.Remove("OrderDetails");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Calculate discount
                        decimal discount = 0;
                        var discountStr = HttpContext.Session.GetString(GetDiscountAmountKey());
                        if (!string.IsNullOrEmpty(discountStr) && decimal.TryParse(discountStr, out var d))
                        {
                            discount = d;
                        }
                        
                        var totalAmount = cart.Sum(i => i.TotalPrice) - discount;
                        if (totalAmount < 0) totalAmount = 0;

                        var order = new Order
                        {
                            UserId = user?.Id,
                            FullName = model.FullName,
                            Address = model.Address,
                            PhoneNumber = model.PhoneNumber,
                            Notes = model.Notes,
                            OrderDate = DateTime.Now,
                            Status = "Pending",
                            TotalAmount = totalAmount
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

                        // Create PaymentTransaction
                        var transactionCode = "TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                        var paymentTransaction = new PaymentTransaction
                        {
                            OrderId = order.Id,
                            OrderCode = string.IsNullOrEmpty(orderCode) ? $"ORD-{DateTime.Now.Ticks}" : orderCode,
                            PaymentMethod = paymentMethod,
                            Amount = order.TotalAmount,
                            Status = paymentMethod.ToLower() == "cod" ? "pending" : "completed",
                            TransactionCode = transactionCode,
                            CreatedDate = DateTime.Now,
                            CompletedDate = paymentMethod.ToLower() == "cod" ? null : DateTime.Now,
                            Notes = $"Thanh toán cho đơn hàng {order.Id} qua {paymentMethod.ToUpper()}"
                        };
                        _context.PaymentTransactions.Add(paymentTransaction);

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        // Clear cart
                        HttpContext.Session.Remove(GetCartKey());

                        // Clear Promo Code session
                        HttpContext.Session.Remove(GetPromoCodeKey());
                        HttpContext.Session.Remove(GetDiscountAmountKey());

                        TempData["SuccessMessage"] = "Đặt hàng thành công!";
                        return RedirectToAction("History", "Order");
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        var errorMessage = "Đã có lỗi xảy ra khi lưu đơn hàng. Vui lòng thử lại. " + ex.Message;
                        if (ex.InnerException != null)
                        {
                            errorMessage += " Details: " + ex.InnerException.Message;
                        }
                        ModelState.AddModelError("", errorMessage);
                    }
                }
            }

            // Recalculate if validation fails
            decimal failDiscount = 0;
            var failDiscountStr = HttpContext.Session.GetString(GetDiscountAmountKey());
            if (!string.IsNullOrEmpty(failDiscountStr) && decimal.TryParse(failDiscountStr, out var fd))
            {
                failDiscount = fd;
            }
            var failTotal = cart.Sum(i => i.TotalPrice) - failDiscount;
            if (failTotal < 0) failTotal = 0;
            model.TotalAmount = failTotal;

            return View(model);
        }

        // POST: /Cart/ApplyPromoCodeJson
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ApplyPromoCodeJson(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá." });
            }

            var cart = GetCart();
            if (!cart.Any())
            {
                return Json(new { success = false, message = "Giỏ hàng của bạn đang trống." });
            }

            var cleanCode = code.Trim().ToUpper();
            var totalAmount = cart.Sum(i => i.TotalPrice);
            decimal discountAmount = 0;
            string message = "";

            switch (cleanCode)
            {
                case "LUCKY10":
                case "AIKHACHHANG":
                    discountAmount = totalAmount * 0.10m; // 10%
                    message = $"Áp dụng thành công mã {cleanCode}: Giảm 10% đơn hàng!";
                    break;
                case "LUCKY20":
                    discountAmount = totalAmount * 0.20m; // 20%
                    message = $"Áp dụng thành công mã {cleanCode}: Giảm 20% đơn hàng!";
                    break;
                case "LUCKY50K":
                    discountAmount = 50000;
                    message = $"Áp dụng thành công mã {cleanCode}: Giảm 50.000đ!";
                    break;
                case "FREESHIP":
                    discountAmount = 0; // Shipping is already free
                    message = $"Áp dụng thành công mã {cleanCode}: Miễn phí vận chuyển!";
                    break;
                default:
                    return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn." });
            }

            if (discountAmount > totalAmount)
            {
                discountAmount = totalAmount;
            }

            // Save to Session
            HttpContext.Session.SetString(GetPromoCodeKey(), cleanCode);
            HttpContext.Session.SetString(GetDiscountAmountKey(), discountAmount.ToString());

            var newTotal = totalAmount - discountAmount;
            if (newTotal < 0) newTotal = 0;

            return Json(new { 
                success = true, 
                message = message, 
                discountAmount = (double)discountAmount, 
                newTotal = (double)newTotal,
                code = cleanCode
            });
        }
    }
}
