﻿namespace SmartFarmManager.API.Payloads.Requests.HealthLog
{
    public class CreateHealthLogRequest
    {
        public Guid? PrescriptionId { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public string Photo { get; set; }
        public Guid TaskId { get; set; }
    }
}