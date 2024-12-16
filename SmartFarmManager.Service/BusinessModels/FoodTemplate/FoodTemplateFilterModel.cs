using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.FoodTemplate
{
    public class FoodTemplateFilterModel
    {
        public Guid? StageTemplateId { get; set; }
        public string? FoodName { get; set; }
        public int? Session { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

}
