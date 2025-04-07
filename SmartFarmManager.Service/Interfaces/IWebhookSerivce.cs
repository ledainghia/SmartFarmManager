using SmartFarmManager.Service.BusinessModels.Webhook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IWebhookService
    {
        
        Task HandleWebhookDataAsync( string dataType ,string jsonRequest);
        Task<bool> ValidateApiKeyAsync(string apiKey, string domain);
    }
}
