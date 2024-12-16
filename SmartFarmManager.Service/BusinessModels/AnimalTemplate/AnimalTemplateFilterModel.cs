using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.AnimalTemplate
{
    public class AnimalTemplateFilterModel
    {
        public string? Name { get; set; }
        public string? Species { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
