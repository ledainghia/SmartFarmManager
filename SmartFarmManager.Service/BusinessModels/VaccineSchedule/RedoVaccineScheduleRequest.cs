﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.VaccineSchedule
{
    public class RedoVaccineScheduleRequest
    {
        public Guid VaccineScheduleId { get; set; }
        public DateTime Date { get; set; }
    }

}
