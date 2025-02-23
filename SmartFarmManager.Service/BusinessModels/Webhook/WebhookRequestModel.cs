using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Webhook
{
    public class WebhookRequestModel
    {
        public string Datatype { get; set; }
        public Object Data { get; set; }

    }
}
