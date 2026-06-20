using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tuan6.Services;
using Tuan6.Models;
using System.Threading.Tasks;

namespace Tuan6.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;
        private readonly ApplicationDbContext _context;

        public PaymentController(
            IPaymentService paymentService, 
            ILogger<PaymentController> logger,
            ApplicationDbContext context)
        {
            _paymentService = paymentService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Generate QR code cho thanh toán
        /// </summary>
        [HttpPost("generate-qr")]
        public IActionResult GenerateQR([FromBody] PaymentQRRequest request)
        {
            try
            {
                if (request.Amount <= 0)
                {
                    return BadRequest(new { error = "Amount must be greater than 0" });
                }

                string qrUrl = string.Empty;

                if (request.Method.ToLower() == "vnpay")
                {
                    qrUrl = _paymentService.GenerateVNPayQR(
                        request.OrderCode,
                        request.Amount,
                        request.BankAccount ?? "");
                }
                else if (request.Method.ToLower() == "momo")
                {
                    qrUrl = _paymentService.GenerateMomoQR(
                        request.OrderCode,
                        request.Amount);
                }
                else
                {
                    return BadRequest(new { error = "Invalid payment method" });
                }

                return Ok(new
                {
                    success = true,
                    method = request.Method,
                    qrUrl = qrUrl,
                    orderCode = request.OrderCode,
                    amount = request.Amount,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating QR: {ex.Message}");
                return StatusCode(500, new { error = "An error occurred while generating QR code" });
            }
        }

        /// <summary>
        /// Lấy hình ảnh QR code từ URL
        /// </summary>
        [HttpGet("qr-image")]
        public async Task<IActionResult> GetQRImage([FromQuery] string method, [FromQuery] string orderCode, [FromQuery] decimal amount)
        {
            try
            {
                string qrUrl = string.Empty;

                if (method.ToLower() == "vnpay")
                {
                    qrUrl = _paymentService.GenerateVNPayQR(orderCode, amount);
                }
                else if (method.ToLower() == "momo")
                {
                    qrUrl = _paymentService.GenerateMomoQR(orderCode, amount);
                }

                if (string.IsNullOrEmpty(qrUrl))
                {
                    return BadRequest("Invalid payment method");
                }

                // Fetch QR image từ URL
                using (var client = new HttpClient())
                {
                    var imageBytes = await client.GetByteArrayAsync(qrUrl);
                    return File(imageBytes, "image/png");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching QR image: {ex.Message}");
                return StatusCode(500, "An error occurred");
            }
        }
    }

    public class PaymentQRRequest
    {
        public string Method { get; set; } = string.Empty; // "vnpay" or "momo"
        public string OrderCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? BankAccount { get; set; }
    }
}
