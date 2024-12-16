using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Cages
{
    public class CageModel
    {
        public Guid Id { get; set; }
        public Guid FarmId { get; set; }
        public string Name { get; set; }
        public double Area { get; set; }
        public int Capacity { get; set; }
        public string Location { get; set; }
        public string AnimalType { get; set; }
    }

}
