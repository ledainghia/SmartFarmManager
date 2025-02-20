using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Webhook;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class WebhookService : IWebhookService
    {
        private IUnitOfWork _unitOfWork;
        private readonly ILogger<WebhookService> _logger;

        public WebhookService(ILogger<WebhookService> logger,IUnitOfWork unitOfWork)
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

        public async Task HandleWebhookDataAsync(WebhookRequestModel webhookRequest)
        {
            switch (webhookRequest.Datatype)
            {
                case "SensorDataOfFarm":
                    _logger.LogInformation("📡 Xử lý dữ liệu cảm biến: {Data}", webhookRequest.Data);
                    break;

                case "WaterDataOfFarm":
                    _logger.LogInformation("💧 Xử lý dữ liệu nước: {Data}", webhookRequest.Data);
                    break;

                case "ElectricDataOfFarm":
                    _logger.LogInformation("⚡ Xử lý dữ liệu điện: {Data}", webhookRequest.Data);
                    break;

                default:
                    _logger.LogWarning("❌ Datatype không hợp lệ: {Datatype}", webhookRequest.Datatype);
                    break;
            }


        }
}
}
