using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Webhook;
using SmartFarmManager.Service.Helpers;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class WebhookService : IWebhookService
    {
        private IUnitOfWork _unitOfWork;
        private readonly ILogger<WebhookService> _logger;

        public WebhookService(ILogger<WebhookService> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> ValidateApiKeyAsync(string apiKey, string domain)
        {
            // Kiểm tra xem domain có hợp lệ không (nằm trong whitelist không)
            var whitelistDomain = await _unitOfWork.WhiteListDomains.FindByCondition(x => x.Domain == domain, false).FirstOrDefaultAsync();
            if (whitelistDomain == null)
            {
                _logger.LogWarning("❌ Domain không hợp lệ: {Domain}", domain);
                return false;
            }

            // Kiểm tra xem API Key có hợp lệ không
            if (whitelistDomain.ApiKey != apiKey)
            {
                _logger.LogWarning("❌ API Key không hợp lệ cho domain {Domain}", domain);
                return false;
            }

            _logger.LogInformation("✅ Domain {Domain} với API Key {ApiKey} đã được xác thực thành công.", domain, apiKey);
            return true;
        }

        public async System.Threading.Tasks.Task HandleWebhookDataAsync(string dataType, string jsonRequest)
        {
            switch (dataType)
            {
                case "SensorDataOfFarm":
                    // _logger.LogInformation("📡 Xử lý dữ liệu cảm biến: {Data}", webhookRequest.Data);
                    var sensorData = JsonConvert.DeserializeObject<SensorDataOfFarmModel>(jsonRequest);
                    if (sensorData != null)
                    {
                        await SaveSensorDataAsync(sensorData);
                    }
                    else
                    {
                        //_logger.LogWarning("❌ Dữ liệu cảm biến không hợp lệ: {Data}", webhookRequest.Data);
                    }
                    break;

                case "WaterDataOfFarm":
                    //_logger.LogInformation("💧 Xử lý dữ liệu nước: {Data}", webhookRequest.Data);
                    break;

                case "ElectricDataOfFarm":
                    //_logger.LogInformation("⚡ Xử lý dữ liệu điện: {Data}", webhookRequest.Data);
                    break;

                default:
                    _logger.LogWarning("❌ Datatype không hợp lệ: {Datatype}", dataType);
                    break;
            }


        }

        private async System.Threading.Tasks.Task SaveSensorDataAsync(SensorDataOfFarmModel sensorData)
        {
            var farm = await _unitOfWork.Farms.FindByCondition(x => x.FarmCode == sensorData.FarmCode, false).FirstOrDefaultAsync();

            if (farm == null)
            {
                _logger.LogWarning("❌ Không tìm thấy trang trại với mã: {FarmCode}", sensorData.FarmCode);
                throw new InvalidOperationException($"Farm với FarmCode {sensorData.FarmCode} không hợp lệ.");
            }

            foreach (var cage in sensorData.Cages)
            {
                var existingCage = await _unitOfWork.Cages.FindByCondition(x => x.PenCode == cage.PenCode && x.FarmId == farm.Id, false).FirstOrDefaultAsync();
                if (existingCage == null)
                {
                    _logger.LogWarning("❌ Không tìm thấy chuồng với mã: {PenCode}", cage.PenCode);
                    throw new InvalidOperationException($"Cage với PenCode {cage.PenCode} không hợp lệ.");
                }
                foreach (var node in cage.Nodes)
                {
                    foreach (var sensor in node.Sensors)
                    {
                        var sensorEntity = await _unitOfWork.Sensors.FindByCondition(s => s.NodeId == node.NodeId && sensor.PinCode == s.PinCode).FirstOrDefaultAsync();
                        if (sensorEntity == null)
                        {
                            _logger.LogWarning("❌ Không tìm thấy cảm biến với PinCode: {PinCode} trong NodeId {NodeId}", sensor.PinCode, node.NodeId);
                            continue; 
                        }
                        var existingDataLog = await _unitOfWork.SensorDataLogs.FindByCondition(sd => sd.SensorId == sensorEntity.Id &&
                                                                                                  sd.CreatedDate.Date == DateTimeUtils.GetServerTimeInVietnamTime().Date).FirstOrDefaultAsync();

                        if (existingDataLog != null)
                        {
                            var existingData = JsonConvert.DeserializeObject<List<SensorDataRecord>>(existingDataLog.Data) ?? new List<SensorDataRecord>();
                            existingData.Add(new SensorDataRecord
                            {
                                Time = DateTimeUtils.GetServerTimeInVietnamTime(),
                                Value = sensor.Value
                            });
                            existingDataLog.Data = JsonConvert.SerializeObject(existingData);
                            await _unitOfWork.SensorDataLogs.UpdateAsync(existingDataLog);
                        }
                        else
                        {
                            var sensorDataLog = new SensorDataLog
                            {
                                SensorId = sensorEntity.Id,
                                Data = JsonConvert.SerializeObject(new List<SensorDataRecord>
                        {
                            new SensorDataRecord
                            {
                                Time = DateTimeUtils.GetServerTimeInVietnamTime(),
                                Value = sensor.Value
                            }
                        }),
                                CreatedDate = DateTimeUtils.GetServerTimeInVietnamTime(),
                                IsWarning = sensor.IsWarning
                            };

                            await _unitOfWork.SensorDataLogs.CreateAsync(sensorDataLog);
                        }
                    }
                }
            }           
            await _unitOfWork.CommitAsync();
            _logger.LogInformation("✅ Dữ liệu cảm biến đã được lưu thành công.");
        }

    }
}
