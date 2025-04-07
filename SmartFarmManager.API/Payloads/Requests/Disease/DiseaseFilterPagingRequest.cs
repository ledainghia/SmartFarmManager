namespace SmartFarmManager.API.Payloads.Requests.Disease
{
    public class DiseaseFilterPagingRequest
    {
        public string? KeySearch { get; set; }
        public bool IsDeleted { get; set; }
        public int PageNumber { get; set; }= 1;
        public int PageSize { get; set; } = 1000;
    }
}
