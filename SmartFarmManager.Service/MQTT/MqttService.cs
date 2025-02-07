using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Client;
using MQTTnet;
using SmartFarmManager.Service.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.MQTT
{
    public class MqttService : IMqttService
    {
        private readonly MqttClientSetting _setting;
        private readonly IMqttClient _mqttClient;
        private readonly ILogger<MqttService> _logger;

        public event EventHandler<string> OnMessageReceived;

        public bool IsConnected => _mqttClient.IsConnected;

        public MqttService(IOptions<MqttClientSetting> options, ILogger<MqttService> logger)
        {
            _setting = options.Value;
            _logger = logger;
            _mqttClient = new MqttFactory().CreateMqttClient();
        }

        // Cấu hình MQTT client
        private MqttClientOptions ConfigureMqttClientOptions()
        {
            return new MqttClientOptionsBuilder()
                .WithClientId(_setting.ClientId)
                .WithTcpServer(_setting.BrokerAddress, _setting.Port)
                .WithCredentials(_setting.UserName, _setting.Password)
                .WithCleanSession(_setting.CleanSession)
                .WithKeepAlivePeriod(TimeSpan.FromSeconds(_setting.KeepAlive))
                .Build();
        }

        // Kết nối tới MQTT broker
        public async Task<bool> ConnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to MQTT broker at {BrokerAddress}:{Port}",
                    _setting.BrokerAddress, _setting.Port);

                var options = ConfigureMqttClientOptions();
                await _mqttClient.ConnectAsync(options, cancellationToken);

                _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceivedHandler;

                _logger.LogInformation("Connected to MQTT broker");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to MQTT broker");
                return false;
            }
        }

        // Đăng ký topic để nhận thông điệp
        public async Task<bool> SubscribeTopicAsync(string topic, CancellationToken cancellationToken)
        {
            try
            {
                if (!IsConnected) return false;

                _logger.LogInformation("Subscribing to topic {Topic}", topic);
                await _mqttClient.SubscribeAsync(new MqttTopicFilter
                {
                    Topic = topic,
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce
                }, cancellationToken);

                _logger.LogInformation("Successfully subscribed to topic {Topic}", topic);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to topic {Topic}", topic);
                return false;
            }
        }

        // Gửi thông điệp đến topic
        public async Task<bool> PublishMessageAsync(string topic, string payload, CancellationToken cancellationToken)
        {
            try
            {
                if (!IsConnected) return false;

                var message = new MqttApplicationMessage
                {
                    Topic = topic,
                    PayloadSegment = Encoding.UTF8.GetBytes(payload),
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce,
                };

                await _mqttClient.PublishAsync(message, cancellationToken);
                _logger.LogInformation("Published to topic {Topic}", topic);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing to topic {Topic}", topic);
                return false;
            }
        }

        // Ngắt kết nối
        public async Task<bool> DisconnectAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!IsConnected) return false;

                _logger.LogInformation("Attempting to disconnect from MQTT broker...");
                var disconnectOptions = new MqttClientDisconnectOptions
                {
                    Reason = MqttClientDisconnectOptionsReason.NormalDisconnection
                };

                // Ngắt kết nối với MQTT broker
                await _mqttClient.DisconnectAsync(disconnectOptions, cancellationToken);
                _logger.LogInformation("Successfully disconnected from MQTT broker");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disconnecting from MQTT broker");
                return false;
            }
        }
        public async Task<bool> UnsubscribeTopicAsync(string topic, CancellationToken cancellationToken)
        {
            try
            {
                if (!IsConnected) return false;

                _logger.LogInformation("Unsubscribing from topic {Topic}", topic);
                await _mqttClient.UnsubscribeAsync(topic, cancellationToken);
                _logger.LogInformation("Successfully unsubscribed from topic {Topic}", topic);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unsubscribing from topic {Topic}", topic);
                return false;
            }
        }

        // Nhận thông điệp từ MQTT broker
        private Task OnMessageReceivedHandler(MqttApplicationMessageReceivedEventArgs e)
        {
            // Lấy thông điệp MQTT và nội dung của nó
            var message = e.ApplicationMessage;
            var payload = Encoding.UTF8.GetString(message.PayloadSegment);  // Chuyển payload từ byte sang string

            // Lấy topic mà thông điệp đã được gửi tới
            var topic = message.Topic ?? string.Empty;

            // Log thông tin thông điệp nhận được
            _logger.LogInformation("Received message from topic: {Topic}, Message: {Message}", topic, payload);

            // Gọi event để thông báo cho các phần khác trong hệ thống về thông điệp
            OnMessageReceived?.Invoke(this, payload);

            return Task.CompletedTask;
        }
    }
}
