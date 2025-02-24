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
                    var waterData = JsonConvert.DeserializeObject<WaterDataOfFarmModel>(jsonRequest);
                    if (waterData != null)
                    {
                        // Gọi phương thức lưu dữ liệu điện vào trong model
                        await SaveWaterDataAsync(waterData);
                    }
                    break;

                case "ElectricDataOfFarm":
                    var electricData = JsonConvert.DeserializeObject<ElectricDataOfFarmModel>(jsonRequest);
                    if (electricData != null)
                    {
                        // Gọi phương thức lưu dữ liệu điện vào trong model
                        await SaveElectricDataAsync(electricData);
                    }
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
        public async System.Threading.Tasks.Task SaveElectricDataAsync(ElectricDataOfFarmModel electricData)
        {
            var farm = await _unitOfWork.Farms.FindByCondition(x => x.FarmCode == electricData.FarmCode, false).FirstOrDefaultAsync();
            if (farm == null)
            {
                _logger.LogWarning("❌ Không tìm thấy farm với FarmCode: {FarmCode}", electricData.FarmCode);
                throw new InvalidOperationException($"Farm với FarmCode {electricData.FarmCode} không hợp lệ.");
            }

            var existingElectricLog = await _unitOfWork.ElectricityLogs
                .FindByCondition(e => e.FarmId == farm.Id && e.CreatedDate.Date == DateTimeUtils.GetServerTimeInVietnamTime().Date)
                .FirstOrDefaultAsync();

            decimal totalConsumption = (decimal)electricData.Data.Sum(record => record.Value);

            if (existingElectricLog == null)
            {
                // Nếu chưa có ElectricityLog cho ngày hôm nay, tạo mới
                var electricityLog = new ElectricityLog
                {
                    FarmId = farm.Id,
                    Data = JsonConvert.SerializeObject(electricData.Data), // Lưu danh sách các record điện dưới dạng JSON
                    TotalConsumption = totalConsumption,  // Tổng điện tiêu thụ của ngày
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.ElectricityLogs.CreateAsync(electricityLog);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("✅ Tạo mới ElectricityLog cho FarmId: {FarmId} với tổng tiêu thụ: {TotalConsumption} kWh", farm.Id, totalConsumption);
            }
            else
            {
                var existingData = JsonConvert.DeserializeObject<List<ElectricRecordModel>>(existingElectricLog.Data);

                // Cập nhật tổng tiêu thụ
                existingElectricLog.TotalConsumption += totalConsumption;

                // Thêm các record mới vào Data
                existingData.AddRange(electricData.Data);

                // Cập nhật lại Data trong ElectricityLog
                existingElectricLog.Data = JsonConvert.SerializeObject(existingData);

                await _unitOfWork.ElectricityLogs.UpdateAsync(existingElectricLog);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("✅ Cập nhật ElectricityLog cho FarmId: {FarmId} với tổng tiêu thụ mới: {TotalConsumption} kWh", farm.Id, existingElectricLog.TotalConsumption);
            }

        }


        private async System.Threading.Tasks.Task SaveWaterDataAsync(WaterDataOfFarmModel waterData)
        {
            var farm = await _unitOfWork.Farms.FindByCondition(x => x.FarmCode == waterData.FarmCode, false).FirstOrDefaultAsync();
            if (farm == null)
            {
                _logger.LogWarning("❌ Không tìm thấy farm với FarmCode: {FarmCode}", waterData.FarmCode);
                throw new InvalidOperationException($"Farm với FarmCode {waterData.FarmCode} không hợp lệ.");
            }

            var exisingWaterLog = await _unitOfWork.WaterLogs
                .FindByCondition(w => w.FarmId == farm.Id && w.CreatedDate.Date == DateTimeUtils.GetServerTimeInVietnamTime().Date)
                .FirstOrDefaultAsync();
            decimal totalConsumption = waterData.Data.Sum(record => (decimal?)record.Value ?? 0);

            if (exisingWaterLog == null)
            {
                var waterLog = new WaterLog
                {
                    FarmId = farm.Id,
                    Data = JsonConvert.SerializeObject(waterData.Data),
                    TotalConsumption = totalConsumption,
                    CreatedDate = DateTimeUtils.GetServerTimeInVietnamTime()
                };
                await _unitOfWork.WaterLogs.CreateAsync(waterLog);
                await _unitOfWork.CommitAsync();

            }else
            {
                var existingData = JsonConvert.DeserializeObject<List<WaterRecordModel>>(exisingWaterLog.Data);
                exisingWaterLog.TotalConsumption += totalConsumption;
                exisingWaterLog.Data = JsonConvert.SerializeObject(existingData);
                await _unitOfWork.WaterLogs.UpdateAsync(exisingWaterLog);
                await _unitOfWork.CommitAsync();
                _logger.LogInformation("✅ Cập nhật WaterLog cho FarmId: {FarmId} với tổng tiêu thụ mới: {TotalConsumption} m³", farm.Id, exisingWaterLog.TotalConsumption);
            }

        }

    }
}
