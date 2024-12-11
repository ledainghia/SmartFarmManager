using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Picture
{
    public class PictureModel
    {
        public Guid Id { get; set; }
        public Guid RecordId { get; set; }

        public string Image { get; set; }

        public DateTime? DateCaptured { get; set; }
    }
}
