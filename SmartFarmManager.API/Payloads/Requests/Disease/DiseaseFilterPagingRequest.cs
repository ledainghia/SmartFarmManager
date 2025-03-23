namespace SmartFarmManager.API.Payloads.Requests.Disease
{
    public class DiseaseFilterPagingRequest
    {
        public string? Name { get; set; }
        public int PageNumber { get; set; }= 1;
        public int PageSize { get; set; } = 1000;
    }
}
