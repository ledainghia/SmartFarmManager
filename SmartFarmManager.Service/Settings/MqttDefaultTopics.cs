using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Settings
{
    public static class MqttDefaultTopics
    {
        /// <summary>
        /// {macAddress}/{penCode[0->15]}/{nodeId[0-15]}/sensors/request
        /// </summary>
        public const string NodeSensorDataResponseTopic = "+/+/+/sensors/response";

        /// <summary>
        /// {macAdress}/controls/response
        /// </summary>
        public const string FarmControlStateResponseTopic = "+/controls/response";

        /// <summary>
        /// {macAdress}/controls/{pin}/response
        /// </summary>
        public const string SingleControlStateResponseTopic = "+/controls/+/response";

        /// <summary>
        /// {macAddress}/power-average/response
        /// </summary>
        public const string ElectricityResponseTopic = "+/power-average/response";

        /// <summary>
        /// {macAddress}/power/response
        /// </summary>
        public const string LastElectricityIndexResponseTopic = "+/power/response";

        /// <summary>
        /// {macAddress}/water-average/response
        /// </summary>
        public const string WaterResponseTopic = "+/water-average/response";

        /// <summary>
        /// {macAddress}/water/response
        /// </summary>
        public const string LastWaterIndexResponseTopic = "+/water/response";

        /// <summary>
        /// {macAddress}/controls/{pin}/request
        /// </summary>
        /// <param name="macAddress"></param>
        /// <param name="pin"></param>
        /// <returns></returns>
        public static string GetSingleControlStateRequestTopic(string macAddress, int pin) =>
            $"{macAddress}/controls/{pin}/request";

        /// <summary>
        /// {macAddress}/controls/request
        /// </summary>
        /// <param name="macAddress"></param>
        /// <returns></returns>
        public static string GetFarmControlStateRequestTopic(string macAddress) =>
            $"{macAddress}/controls/request";

        /// <summary>
        /// {macAddress}/controls/{pinCode}/command
        /// </summary>
        /// <param name="macAddress"></param>
        /// <param name="pinCode"></param>
        /// <returns></returns>
        public static string GetSendCommandTopic(string macAddress, int pinCode) =>
            $"{macAddress}/controls/{pinCode}/command";

        /// <summary>
        /// sensors/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string GetRealTimeSensorDataTopic(Guid id) => $"sensors/{id}";

        /// <summary>
        /// {macAddress}/{penCode}/{nodeId}/sensors/request
        /// </summary>
        /// <param name="macAddress"></param>
        /// <param name="penCode"></param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public static string GetNodeSensorDataRequestTopic(string macAddress, string penCode, int nodeId) =>
            $"{macAddress}/{penCode}/{nodeId}/sensors/request";

        public static string GetResetWifiTopic(string macAddress) => $"{macAddress}/reset-wifi/request";
        public static string GetElectricityRequestTopic(string macAddress) => $"{macAddress}/power/request";
        public static string GetWaterRequestTopic(string macAddress) => $"{macAddress}/water/request";
    }
}
