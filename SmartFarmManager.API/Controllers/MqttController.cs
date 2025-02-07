using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartFarmManager.API.Payloads.Requests.MQTT;
using SmartFarmManager.Service.MQTT;

namespace SmartFarmManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MqttController : ControllerBase
    {
        private readonly IMqttService _mqttService;
        private readonly ILogger<MqttController> _logger;

        public MqttController(IMqttService mqttService, ILogger<MqttController> logger)
        {
            _mqttService = mqttService;
            _logger = logger;
        }

        // Kết nối MQTT broker
        [HttpPost("connect")]
        public async Task<IActionResult> Connect(CancellationToken cancellationToken)
        {
            var result = await _mqttService.ConnectAsync(cancellationToken);
            if (!result)
            {
                _logger.LogError("Failed to connect to MQTT broker.");
                return BadRequest("Failed to connect to MQTT broker.");
            }
            _logger.LogInformation("Successfully connected to MQTT broker.");
            return Ok("Connected to MQTT broker.");
        }

        // Ngắt kết nối MQTT broker
        [HttpPost("disconnect")]
        public async Task<IActionResult> Disconnect(CancellationToken cancellationToken)
        {
            var result = await _mqttService.DisconnectAsync(cancellationToken);
            if (!result)
            {
                _logger.LogError("Failed to disconnect from MQTT broker.");
                return BadRequest("Failed to disconnect from MQTT broker.");
            }
            _logger.LogInformation("Successfully disconnected from MQTT broker.");
            return Ok("Disconnected from MQTT broker.");
        }

        // Đăng ký topic để nhận thông điệp
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] string topic, CancellationToken cancellationToken)
        {
            var result = await _mqttService.SubscribeTopicAsync(topic, cancellationToken);
            if (!result)
            {
                _logger.LogError($"Failed to subscribe to topic {topic}.");
                return BadRequest($"Failed to subscribe to topic {topic}.");
            }
            _logger.LogInformation($"Successfully subscribed to topic {topic}.");
            return Ok($"Subscribed to topic {topic}.");
        }

        // Hủy đăng ký topic
        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] string topic, CancellationToken cancellationToken)
        {
            var result = await _mqttService.UnsubscribeTopicAsync(topic, cancellationToken);
            if (!result)
            {
                _logger.LogError($"Failed to unsubscribe from topic {topic}.");
                return BadRequest($"Failed to unsubscribe from topic {topic}.");
            }
            _logger.LogInformation($"Successfully unsubscribed from topic {topic}.");
            return Ok($"Unsubscribed from topic {topic}.");
        }

        // Gửi thông điệp đến một topic
        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] PublishRequest request, CancellationToken cancellationToken)
        {
            var result = await _mqttService.PublishMessageAsync(request.Topic, request.Payload, cancellationToken);
            if (!result)
            {
                _logger.LogError($"Failed to publish message to topic {request.Topic}.");
                return BadRequest($"Failed to publish message to topic {request.Topic}.");
            }
            _logger.LogInformation($"Successfully published message to topic {request.Topic}.");
            return Ok($"Message published to topic {request.Topic}.");
        }
    }
}
