using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public partial class EggHarvest :EntityBase
    {
        public  Guid GrowthStageId { get; set; }
        public DateTime DateCollected { get; set; }
        public int EggCount { get; set; }
        public string? Notes { get; set; }
        [ForeignKey(nameof(GrowthStageId))]
        public virtual GrowthStage growthStage { get; set; }
    }
}
