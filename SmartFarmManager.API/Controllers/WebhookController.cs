using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Payloads.Requests.Webhook;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IWebhookService _webhookService;
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(IWebhookService webhookService, ILogger<WebhookController> logger)
        {
            _webhookService = webhookService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveData([FromBody] WebhookRequest webhookRequest)
        {
            // Kiểm tra `x-api-key`
            if (!Request.Headers.ContainsKey("x-api-key") || string.IsNullOrEmpty(Request.Headers["x-api-key"].ToString()))
            {
                _logger.LogWarning("❌ Thiếu x-api-key trong header.");
                return Unauthorized("Thiếu x-api-key.");
            }

            var apiKey = Request.Headers["x-api-key"].ToString();
            //var domain = Request.Headers["Origin"].ToString() ?? Request.Headers["Referer"].ToString();
            var domain = Request.Host.Host;

            if (string.IsNullOrEmpty(domain))
            {
                _logger.LogWarning("❌ Thiếu domain trong header.");
                return BadRequest("Thiếu domain.");
            }

            // Validate `x-api-key` và `domain` qua service
            var isValid = await _webhookService.ValidateApiKeyAsync(apiKey, domain);
            if (!isValid)
            {
                return Unauthorized("API Key hoặc Domain không hợp lệ.");
            }

            _logger.LogInformation("✅ Nhận dữ liệu webhook từ domain {Domain}", domain);
            _logger.LogInformation("🔹 Datatype: {Datatype}", webhookRequest.Datatype);
            _logger.LogInformation("🔹 Data: {Data}", System.Text.Json.JsonSerializer.Serialize(webhookRequest.Data));

            // Xử lý dữ liệu (nếu cần)
            await _webhookService.HandleWebhookDataAsync(webhookRequest.MapToModel());

            return Ok("✅ Dữ liệu đã được nhận thành công!");
        }
    }

}
