using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.AnimalTemplate
{
    public class CreateAnimalTemplateModel
    {
        public string Name { get; set; }
        public string Species { get; set; }
        public int? DefaultCapacity { get; set; }
        public string Notes { get; set; }
    }
}
