using SmartFarmManager.Service.BusinessModels.Webhook;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmManager.API.Payloads.Requests.Webhook
{
    public class WebhookRequest
    {
        [Required]
        public string Datatype { get; set; }
        [Required]
        public object Data { get; set; }

        public WebhookRequestModel MapToModel()
        {
            return new WebhookRequestModel
            {
                Datatype = Datatype,
                Data = Data
            };
        }
    }
}
