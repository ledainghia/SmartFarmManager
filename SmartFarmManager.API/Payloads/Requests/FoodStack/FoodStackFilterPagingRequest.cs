namespace SmartFarmManager.API.Payloads.Requests.FoodStack
{
    public class FoodStackFilterPagingRequest
    {
        public string? KeySearch { get; set; }
        public Guid? FarmId { get; set; }
        public string? FoodType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool? IsDeleted { get; set; }
    }
}
