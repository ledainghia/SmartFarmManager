using Sprache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.MQTT
{
    public interface IMqttService
    {
        Task<bool> ConnectAsync(CancellationToken cancellationToken);
        Task<bool> DisconnectAsync(CancellationToken cancellationToken);
        Task<bool> SubscribeTopicAsync(string topic, CancellationToken cancellationToken);
        Task<bool> UnsubscribeTopicAsync(string topic, CancellationToken cancellationToken);
        Task<bool> PublishMessageAsync(string topic, string payload, CancellationToken cancellationToken);
        event EventHandler<string> OnMessageReceived;  // Để nhận thông điệp từ MQTT broker
    }

}
