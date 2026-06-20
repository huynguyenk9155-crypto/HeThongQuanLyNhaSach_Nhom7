using Tuan6.Repositories;

namespace Tuan6.Services
{
    public interface IChatbotService
    {
        Task<string> GetAIResponseAsync(string userMessage, string conversationContext = "");
    }

    public class ChatbotService : IChatbotService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChatbotService> _logger;
        private readonly IBookRepository _bookRepository;

        public ChatbotService(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<ChatbotService> logger,
            IBookRepository bookRepository)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _bookRepository = bookRepository;
        }

        public async Task<string> GetAIResponseAsync(string userMessage, string conversationContext = "")
        {
            try
            {
                var apiProvider = _configuration["Chatbot:Provider"] ?? "openai"; // "openai" hoặc "gemini"
                
                if (apiProvider.ToLower() == "openai")
                {
                    return await GetOpenAIResponseAsync(userMessage, conversationContext);
                }
                else if (apiProvider.ToLower() == "gemini")
                {
                    return await GetGeminiResponseAsync(userMessage, conversationContext);
                }
                
                // Fallback: Trả lời mặc định
                return GetDefaultResponse(userMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting AI response: {ex.Message}");
                return "Xin lỗi, tôi không thể xử lý câu hỏi của bạn lúc này. Vui lòng liên hệ hỗ trợ khách hàng.";
            }
        }

        private async Task<string> GetSystemPromptWithBooksAsync()
        {
            var basePrompt = @"Bạn là một trợ lý khách hàng cho cửa hàng sách trực tuyến Tuan6 Bookstore. 
Bạn giúp khách hàng:
- Tìm kiếm sách theo tên, tác giả, hoặc thể loại
- Trả lời câu hỏi về sản phẩm, giá cả, thông tin giao hàng
- Giải quyết vấn đề về đơn hàng
- Cung cấp thông tin về khuyến mãi và ưu đãi
- Hỗ trợ quy trình thanh toán

Hãy trả lời ngắn gọn, thân thiện và hữu ích. Luôn sử dụng tiếng Việt.";

            try
            {
                var books = await _bookRepository.GetAllAsync();
                if (books != null && books.Any())
                {
                    var booksListText = string.Join("\n", books.Select(b => 
                        $"- Mã ID: {b.Id} | Tiêu đề: \"{b.Title}\" | Tác giả: {b.Author} | Giá: {b.Price:N0}đ | Số lượng còn lại: {b.StockQuantity} | Thể loại: {b.Category?.Name ?? "Khác"}{(string.IsNullOrEmpty(b.Description) ? "" : " | Mô tả: " + b.Description)}"));

                    return $@"{basePrompt}

Dưới đây là danh sách toàn bộ sách hiện có trong cơ sở dữ liệu của cửa hàng:
{booksListText}

Hãy trả lời và tư vấn khách hàng dựa trên danh sách này:
- Khi khách hàng hỏi mua hoặc tìm sách, hãy trả lời chính xác xem cửa hàng có cuốn đó không, giá bao nhiêu, tác giả là ai và còn hàng hay không.
- Nếu cuốn sách không có trong danh sách trên, hãy phản hồi lịch sự rằng cửa hàng hiện tại chưa có sách này và gợi ý họ tìm cuốn khác, hoặc đề xuất liên hệ nhân viên để đặt trước.
- ĐẶC BIỆT: Nếu khách hàng nói muốn mua sách, muốn đặt hàng, hoặc muốn thêm sách vào giỏ hàng (ví dụ: 'Tôi muốn mua 2 cuốn Đắc Nhân Tâm', 'cho tôi 1 cuốn Nhà Giả Kim', 'thêm cuốn Sapiens vào giỏ giúp tôi'...), hãy trả lời thân thiện xác nhận hành động đó và ĐỒNG THỜI chèn thêm ở CUỐI CÙNG câu trả lời của bạn một dòng duy nhất có cú pháp hành động đặc biệt sau (và CHỈ chứa cú pháp này ở dòng cuối cùng đó, không được kèm bất kỳ ký tự hoặc text nào khác ở cùng dòng đó):
[ACTION: ADD_TO_CART, {{ ""items"": [ {{ ""id"": BOOK_ID, ""quantity"": QUANTITY, ""title"": ""BOOK_TITLE"" }} ] }}]
Trong đó BOOK_ID là mã số (Mã ID) của cuốn sách tương ứng trong danh sách, QUANTITY là số lượng khách muốn mua, BOOK_TITLE là tiêu đề cuốn sách. Hãy chắc chắn điền đúng thông tin. Nếu họ muốn mua nhiều loại sách khác nhau, hãy liệt kê tất cả chúng trong danh sách 'items'. Ví dụ:
[ACTION: ADD_TO_CART, {{ ""items"": [ {{ ""id"": 1, ""quantity"": 2, ""title"": ""Đắc Nhân Tâm"" }}, {{ ""id"": 2, ""quantity"": 1, ""title"": ""Nhà Giả Kim"" }} ] }}]
- KHUYẾN MÃI & MÃ GIẢ GIÁ: 
  + Cửa hàng đang có chương trình Vòng quay may mắn, bạn có thể hướng dẫn khách hàng click vào biểu tượng Hộp quà 'Vòng Quay' ở góc dưới màn hình để quay và nhận các Voucher ngẫu nhiên (LUCKY10, LUCKY20, LUCKY50K, FREESHIP).
  + Nếu khách hàng hỏi xin mã giảm giá, phân vân về giá đắt, hoặc hỏi về ưu đãi đặc biệt, bạn có thể chủ động tặng riêng cho họ mã giảm giá đặc biệt `AIKHACHHANG` (giảm 10% tổng đơn hàng) và hướng dẫn họ nhập mã này tại trang Thanh toán để được áp dụng giảm giá!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading books for chatbot prompt: {ex.Message}");
            }

            return basePrompt;
        }

        private async Task<string> GetOpenAIResponseAsync(string userMessage, string conversationContext)
        {
            var apiKey = _configuration["Chatbot:OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return GetDefaultResponse(userMessage);
            }

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var systemPrompt = await GetSystemPromptWithBooksAsync();

            var messagesList = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            if (!string.IsNullOrEmpty(conversationContext))
            {
                var lines = conversationContext.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var colonIndex = line.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        var roleStr = line.Substring(0, colonIndex).Trim().ToLower();
                        var contentStr = line.Substring(colonIndex + 1).Trim();
                        var role = (roleStr == "user") ? "user" : "assistant";
                        messagesList.Add(new { role = role, content = contentStr });
                    }
                }
            }

            if (messagesList.Count == 1) // If no history, add current user message
            {
                messagesList.Add(new { role = "user", content = userMessage });
            }

            var model = _configuration["Chatbot:OpenAI:Model"] ?? "gpt-3.5-turbo";

            var requestBody = new
            {
                model = model,
                messages = messagesList.ToArray(),
                temperature = 0.7,
                max_tokens = 200
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            var response = await client.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(jsonResponse);
                var content = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
                return content ?? GetDefaultResponse(userMessage);
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError($"OpenAI API error: {response.StatusCode} - {errorResponse}");
            }

            return GetDefaultResponse(userMessage);
        }

        private async Task<string> GetGeminiResponseAsync(string userMessage, string conversationContext)
        {
            var apiKey = _configuration["Chatbot:Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return GetDefaultResponse(userMessage);
            }

            var client = _httpClientFactory.CreateClient();

            var systemPrompt = await GetSystemPromptWithBooksAsync();

            var contentsList = new List<object>();

            if (!string.IsNullOrEmpty(conversationContext))
            {
                var lines = conversationContext.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var colonIndex = line.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        var roleStr = line.Substring(0, colonIndex).Trim().ToLower();
                        var contentStr = line.Substring(colonIndex + 1).Trim();
                        var role = (roleStr == "user") ? "user" : "model";
                        contentsList.Add(new { role = role, parts = new[] { new { text = contentStr } } });
                    }
                }
            }

            if (contentsList.Count == 0) // If no history, add current user message
            {
                contentsList.Add(new { role = "user", parts = new[] { new { text = userMessage } } });
            }

            var model = _configuration["Chatbot:Gemini:Model"] ?? "gemini-2.5-flash";

            var requestBody = new
            {
                contents = contentsList.ToArray(),
                systemInstruction = new { parts = new[] { new { text = systemPrompt } } }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(jsonResponse);
                var content = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
                return content ?? GetDefaultResponse(userMessage);
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Gemini API error: {response.StatusCode} - {errorResponse}");
            }

            return GetDefaultResponse(userMessage);
        }

        private string GetDefaultResponse(string userMessage)
        {
            // Trả lời mặc định dựa trên từ khóa
            if (userMessage.ToLower().Contains("sách") || userMessage.ToLower().Contains("book"))
            {
                return "Chúng tôi có hàng ngàn cuốn sách. Bạn tìm sách về chủ đề nào? 📚";
            }
            else if (userMessage.ToLower().Contains("giá") || userMessage.ToLower().Contains("price"))
            {
                return "Giá sách của chúng tôi rất cạnh tranh. Bạn có thể xem chi tiết từng cuốn trên website. 💰";
            }
            else if (userMessage.ToLower().Contains("giao") || userMessage.ToLower().Contains("ship"))
            {
                return "Chúng tôi giao hàng miễn phí toàn quốc cho đơn hàng từ 100k. 🚚";
            }
            else if (userMessage.ToLower().Contains("khuyến") || userMessage.ToLower().Contains("giảm"))
            {
                return "Chúng tôi thường xuyên có khuyến mãi. Hãy kiểm tra phần 'Flash Sale' để cập nhật. 🎉";
            }
            else if (userMessage.ToLower().Contains("thanh toán") || userMessage.ToLower().Contains("payment"))
            {
                return "Chúng tôi hỗ trợ thanh toán QR (VNPay, MoMo), chuyển khoản, COD. Bạn chọn hình thức nào? 💳";
            }
            
            return "Xin chào! 👋 Tôi có thể giúp bạn tìm sách, kiểm tra đơn hàng, hoặc trả lời câu hỏi. Bạn cần gì?";
        }
    }
}
