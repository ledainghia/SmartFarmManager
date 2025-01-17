namespace SmartFarmManager.API.Payloads.Requests.FarmingBatch
{
    public class FarmingBatchFilterPagingRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 1000;
        public string? Status { get; set; } // Filter by Status
        public string? CageName { get; set; } // Filter by Cage Name
        public string? Name { get; set; } // Filter by Batch Name
        public DateTime? StartDateFrom { get; set; } // Filter by Start Date Range
        public DateTime? StartDateTo { get; set; }
        public Guid? CageId { get; set; }
    }
}
