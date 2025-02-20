using MQTTnet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Settings
{
    public record MqttClientSetting
    {
        public static readonly string Section = "MqttClientSettings";

        [Required] public string BrokerAddress { get; init; } = null!;
        public int Port { get; init; }
        [Required] public string ClientId { get; init; } = null!;
        [Required] public string UserName { get; init; } = null!;
        [Required] public string Password { get; init; } = null!;
        public int QoS { get; init; }
        public int KeepAlive { get; init; }
        public bool CleanSession { get; init; }
        public bool UseTls { get; init; }
    }

}
