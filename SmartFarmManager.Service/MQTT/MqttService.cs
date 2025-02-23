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
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.Helpers;
using Sprache;

namespace SmartFarmManager.Service.MQTT
{
    public class MqttService : IMqttService
    {
        //    public bool IsConnected => _mqttClient.IsConnected;

        //    private readonly MqttClientSetting _setting;
        //    private readonly IMqttClient _mqttClient;
        //    private readonly ILogger<MqttService> _logger;
        //    private readonly IServiceProvider _serviceProvider;

        //    public event EventHandler<(string macAddress, double data)> OnElectricityDataReceived;
        //    public event EventHandler<(string macAddress, double data)> OnWaterDataReceived;

        //    public MqttService(IOptions<MqttClientSetting> options, ILogger<MqttService> logger, IServiceProvider serviceProvider)
        //    {
        //        _logger = logger;
        //        _serviceProvider = serviceProvider;
        //        _mqttClient = new MqttFactory().CreateMqttClient();
        //        _setting = options.Value;
        //    }

        //    public async System.Threading.Tasks.Task ConnectBrokerAsync(CancellationToken cancellationToken)
        //    {
        //        try
        //        {
        //            var options = new MqttClientOptionsBuilder()
        //                .WithClientId(_setting.ClientId)
        //                .WithTcpServer(_setting.BrokerAddress, _setting.Port)
        //                .WithCredentials(_setting.UserName, _setting.Password)
        //                .WithCleanSession()
        //                .WithKeepAlivePeriod(TimeSpan.FromSeconds(_setting.KeepAlive))
        //                .Build();

        //            _logger.LogInformation("Connecting to MQTT broker...");
        //            await _mqttClient.ConnectAsync(options, cancellationToken);
        //            _logger.LogInformation("MQTT broker connected");

        //            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;
        //            _mqttClient.DisconnectedAsync += async (args) => await Reconnect();
        //           await SubscribeToDefaultTopicsAsync(cancellationToken);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "MQTT broker connection error");
        //        }
        //    }

        //    public async Task DisconnectBrokerAsync(CancellationToken cancellationToken)
        //    {
        //        if (!IsConnected) return;
        //        await _mqttClient.DisconnectAsync(new MqttClientDisconnectOptions(), cancellationToken);
        //        _logger.LogInformation("Disconnected from MQTT broker");
        //    }

        //    public async Task PublishMessageAsync(string topic, string payload, CancellationToken cancellationToken)
        //    {
        //        if (!IsConnected)
        //        {
        //            _logger.LogError("MQTT client is not connected");
        //            return;
        //        }

        //        var message = new MqttApplicationMessageBuilder()
        //            .WithTopic(topic)
        //            .WithPayload(payload)
        //            .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
        //            .Build();

        //        await _mqttClient.PublishAsync(message, cancellationToken);
        //        _logger.LogInformation("Published message to {Topic}", topic);
        //    }

        //    public async System.Threading.Tasks.Task SubscribeTopicAsync(string topic, CancellationToken cancellationToken)
        //    {
        //        if (!IsConnected) return;
        //        await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(topic).Build(), cancellationToken);
        //        _logger.LogInformation("Subscribed to topic {Topic}", topic);
        //    }

        //    public async System.Threading.Tasks.Task SubscribeTopicsAsync(List<string> topics, CancellationToken cancellationToken)
        //    {
        //        if (!IsConnected) return;
        //        foreach (var topic in topics)
        //        {
        //            await SubscribeTopicAsync(topic, cancellationToken);
        //        }
        //    }

        //    public async Task UnsubscribeTopicAsync(string topic, CancellationToken cancellationToken)
        //    {
        //        if (!IsConnected) return;
        //        await _mqttClient.UnsubscribeAsync(topic, cancellationToken);
        //        _logger.LogInformation("Unsubscribed from topic {Topic}", topic);
        //    }

        //    private async Task Reconnect()
        //    {
        //        if (IsConnected) return;
        //        _logger.LogInformation("Reconnecting to MQTT broker...");
        //        await ConnectBrokerAsync(default);
        //    }

        //    private async System.Threading.Tasks.Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs arg)
        //    {
        //        var message = arg.ApplicationMessage;
        //        var topic = message.Topic ?? string.Empty;
        //        var payload = Encoding.UTF8.GetString(message.PayloadSegment);
        //        _logger.LogInformation("Received message from {Topic}: {Payload}", topic, payload);
        //        await RouteTopicHandlerAsync(topic, payload);
        //    }

        //    public void Dispose()
        //    {
        //        _mqttClient.Dispose();
        //        GC.SuppressFinalize(this);
        //    }

        //    public System.Threading.Tasks.Task SendCommandAsync(string macAddress, int pinCode, string command, CancellationToken cancellationToken = default)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    private async System.Threading.Tasks.Task RouteTopicHandlerAsync(string topic, string payload)
        //    {
        //        using var scope = _serviceProvider.CreateScope();

        //        if (topic.MatchMqttPattern(MqttDefaultTopics.NodeSensorDataResponseTopic))
        //        {
        //            _logger.LogInformation("Xử lý dữ liệu cảm biến từ Node");
        //            await HandleNodeSensorDataResponse(topic, payload, scope);
        //        }
        //        else if (topic.MatchMqttPattern(MqttDefaultTopics.ElectricityResponseTopic))
        //        {
        //            _logger.LogInformation("Xử lý dữ liệu điện");
        //            await HandleElectricityResponse(topic, payload, scope);
        //        }
        //        else if (topic.MatchMqttPattern(MqttDefaultTopics.WaterResponseTopic))
        //        {
        //            _logger.LogInformation("Xử lý dữ liệu nước");
        //            await HandleWaterResponse(topic, payload, scope);
        //        }
        //        else
        //        {
        //            _logger.LogWarning("Không xác định topic: {Topic}", topic);
        //        }
        //     }
        //    private Task HandleNodeSensorDataResponse(string topic, string payload)
        //    {
        //        using var scope = _serviceProvider.CreateScope();
        //        var mqttProducer = scope.ServiceProvider.GetRequiredService<IMqttProducerService>();
        //        var message = new MqttRabbitMqMessage
        //        {
        //            Topic = topic,
        //            Payload = payload
        //        };
        //        // let the queue handle the message
        //        mqttProducer.SendMessage(message);
        //        _logger.LogInformation("Sensor data sent to queue");
        //        return Task.CompletedTask;
        //    }

        //    private async Task HandleElectricityResponse(string topic, string payload, IServiceScope scope)
        //    {
        //        var electricityLogRepository = scope.ServiceProvider.GetRequiredService<IElectricityLogRepository>();
        //        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        //        var model = JsonConvert.DeserializeObject<ElectricityWaterModel>(payload);
        //        if (model == null)
        //        {
        //            _logger.LogError("Payload không hợp lệ: {Payload}", payload);
        //            return;
        //        }

        //        var electricityLog = new ElectricityLog
        //        {
        //            Data = model.Data,
        //            BeginTime = model.BeginTime,
        //            EndTime = model.EndTime,
        //            CreatedDate = DateTime.UtcNow
        //        };
        //        electricityLogRepository.Add(electricityLog);
        //        await unitOfWork.SaveChangesAsync();
        //    }

        //    private async Task HandleWaterResponse(string topic, string payload, IServiceScope scope)
        //    {
        //        var waterLogRepository = scope.ServiceProvider.GetRequiredService<IWaterLogRepository>();
        //        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        //        var model = JsonConvert.DeserializeObject<ElectricityWaterModel>(payload);
        //        if (model == null)
        //        {
        //            _logger.LogError("Payload không hợp lệ: {Payload}", payload);
        //            return;
        //        }

        //        var waterLog = new WaterLog
        //        {
        //            Data = model.Data,
        //            BeginTime = model.BeginTime,
        //            EndTime = model.EndTime,
        //            CreatedDate = DateTime.UtcNow
        //        };
        //        waterLogRepository.Add(waterLog);
        //        await unitOfWork.SaveChangesAsync();
        //    }

        //    private async System.Threading.Tasks.Task SubscribeToDefaultTopicsAsync(CancellationToken cancellationToken)
        //    {
        //        try
        //        {
        //            var topics = new List<string>
        //            {
        //        MqttDefaultTopics.NodeSensorDataResponseTopic,
        //        MqttDefaultTopics.FarmControlStateResponseTopic,
        //        MqttDefaultTopics.SingleControlStateResponseTopic,
        //        MqttDefaultTopics.ElectricityResponseTopic,
        //        MqttDefaultTopics.LastElectricityIndexResponseTopic,
        //        MqttDefaultTopics.WaterResponseTopic,
        //        MqttDefaultTopics.LastWaterIndexResponseTopic
        //    };

        //            await SubscribeTopicsAsync(topics, cancellationToken);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error subscribing to topics");
        //        }
        //    }

        //    System.Threading.Tasks.Task IMqttService.DisconnectBrokerAsync(CancellationToken cancellationToken)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    System.Threading.Tasks.Task IMqttService.UnsubscribeTopicAsync(string topic, CancellationToken cancellationToken)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    System.Threading.Tasks.Task IMqttService.PublishMessageAsync(string topic, string payload, CancellationToken cancellationToken)
        //    {
        //        throw new NotImplementedException();
        //    }
        //public void Dispose()
        //{
        //    throw new NotImplementedException();
        //}
    }
}
