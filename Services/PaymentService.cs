using System.Security.Cryptography;
using System.Text;

namespace Tuan6.Services
{
    public interface IPaymentService
    {
        string GenerateVNPayQR(string orderCode, decimal amount, string bankAccount = "");
        string GenerateMomoQR(string orderId, decimal amount);
        bool VerifyVNPayChecksum(Dictionary<string, string> parameters, string secureHash);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Tạo QR Code VNPay
        /// </summary>
        public string GenerateVNPayQR(string orderCode, decimal amount, string bankAccount = "")
        {
            var tmnCode = _configuration["Payment:VNPay:TmnCode"] ?? "TMUDF20240001";
            var returnUrl = _configuration["Payment:VNPay:ReturnUrl"] ?? "https://localhost:7075/payment/vnpay-return";
            var apiUrl = _configuration["Payment:VNPay:ApiUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            
            var requestData = new Dictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", tmnCode },
                { "vnp_Amount", ((long)(amount * 100)).ToString() },
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", "127.0.0.1" },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", $"Thanh toan don hang {orderCode}" },
                { "vnp_OrderType", "other" },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_TxnRef", orderCode }
            };

            if (!string.IsNullOrEmpty(bankAccount))
            {
                requestData.Add("vnp_BankCode", bankAccount);
            }

            // Order query parameters alphabetically as required by VNPay
            var sortedList = requestData.OrderBy(x => x.Key).ToList();
            var dataToHash = string.Join("&", sortedList.Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            
            var hashSecret = _configuration["Payment:VNPay:HashSecret"] ?? "EACTORY@#$%^&*()_+{}|:\"<>?";
            var secureHash = ComputeHMACSHA512(dataToHash, hashSecret);
            
            var paymentUrl = $"{apiUrl}?{dataToHash}&vnp_SecureHash={secureHash}";
            
            // Generate QR code image URL wrapping the VNPay payment link
            var qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=250x250&data={Uri.EscapeDataString(paymentUrl)}";
            return qrUrl;
        }

        /// <summary>
        /// Tạo QR Code MoMo
        /// </summary>
        public string GenerateMomoQR(string orderId, decimal amount)
        {
            // MoMo QR format: https://qr.momo.vn/
            // Định dạng: https://qr.momo.vn/?data=...
            
            var momoNumber = _configuration["Payment:Momo:PhoneNumber"] ?? "0123456789";
            var momoName = _configuration["Payment:Momo:Name"] ?? "Your Shop Name";
            
            // Format MoMo QR data
            var momoData = $"00020101021226660010A000000727{momoNumber}0708QMTQ50300037280010A821059401050000Ftta5303704540{amount:0}5802VN6304";
            
            // Tạo QR URL
            var qrUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=250x250&data={Uri.EscapeDataString(momoData)}";
            return qrUrl;
        }

        /// <summary>
        /// Xác minh checksum VNPay
        /// </summary>
        public bool VerifyVNPayChecksum(Dictionary<string, string> parameters, string secureHash)
        {
            var hashSecret = _configuration["Payment:VNPay:HashSecret"] ?? "EACTORY@#$%^&*()_+{}|:\"<>?";
            
            var dataToHash = string.Join("&", parameters
                .Where(x => !string.IsNullOrEmpty(x.Value))
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Value}"));

            var computedHash = ComputeHMACSHA512(dataToHash, hashSecret);
            return computedHash.Equals(secureHash, StringComparison.OrdinalIgnoreCase);
        }

        private string ComputeSHA256(string data, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        private string ComputeHMACSHA512(string data, string key)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToUpper(); // VNPay secure hash is uppercase hex
            }
        }
    }
}
