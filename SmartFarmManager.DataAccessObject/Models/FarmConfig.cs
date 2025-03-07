using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public class FarmConfig:EntityBase
    {
        public Guid FarmId { get; set; }
        public int MaxCagesPerStaff { get; set; } 
        public int MaxFarmingBatchesPerCage { get; set; } 
        public TimeSpan TimeDifference { get; set; } 
        public virtual Farm Farm { get; set; }
        public DateTime LastTimeUpdated { get; set; } 
    }
}
