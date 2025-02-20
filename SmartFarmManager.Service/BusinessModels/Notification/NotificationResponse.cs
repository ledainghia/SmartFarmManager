﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Notification
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; }
        public string Title {  get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsRead { get; set; }
        public Guid? TaskId { get; set; }
        public Guid? MedicalSymptomId { get; set; }
        public Guid? CageId { get; set; }
    }

}
