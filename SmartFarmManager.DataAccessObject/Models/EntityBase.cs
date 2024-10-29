﻿using SmartFarmManager.DataAccessObject.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.DataAccessObject.Models
{
    public abstract class EntityBase : IEntityBase
    {
        [Key]
        public int Id { get; set; }
    }
}
