﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace SmartFarmManager.DataAccessObject.Models;

    public partial class PrescriptionMedication : EntityBase
    {

        public Guid PrescriptionId { get; set; }

        public Guid MedicationId { get; set; }

        public int Morning { get; set; } = 0;
        public int Afternoon { get; set; } = 0;
        public int Evening { get; set; } = 0;
        public int Noon { get; set; } = 0;

        public virtual Medication Medication { get; set; }

        public virtual Prescription Prescription { get; set; }
    }