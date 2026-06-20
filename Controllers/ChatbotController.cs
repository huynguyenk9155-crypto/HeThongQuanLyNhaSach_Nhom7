using Microsoft.AspNetCore.Mvc;
using Tuan6.Services;

namespace Tuan6.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IChatbotService _chatbotService;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(IChatbotService chatbotService, ILogger<ChatbotController> logger)
        {
            _chatbotService = chatbotService;
            _logger = logger;
        }

        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            if (string.IsNullOrWhiteSpace(message.Message))
            {
                return BadRequest(new { error = "Message cannot be empty" });
            }

            try
            {
                var response = await _chatbotService.GetAIResponseAsync(message.Message, message.Context ?? "");
                
                return Ok(new
                {
                    success = true,
                    message = response,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in chatbot: {ex.Message}");
                return StatusCode(500, new { error = "An error occurred while processing your message" });
            }
        }
    }

    public class ChatMessage
    {
        public string Message { get; set; } = string.Empty;
        public string? Context { get; set; }
    }
}
