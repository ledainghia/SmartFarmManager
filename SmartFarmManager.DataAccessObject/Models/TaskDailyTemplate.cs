using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public class TaskDailyTemplate:EntityBase
    {

        [Required]
        public Guid GrowthStageTemplateId { get; set; }

        public Guid? TaskTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string TaskName { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        public int Session { get; set; }

        [ForeignKey(nameof(GrowthStageTemplateId))]
        public GrowthStageTemplate GrowthStageTemplate { get; set; }
    }
}
