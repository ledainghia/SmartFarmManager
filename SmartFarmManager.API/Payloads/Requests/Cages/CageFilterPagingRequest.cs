using SmartFarmManager.Service.BusinessModels.Cages;

namespace SmartFarmManager.API.Payloads.Requests.Cages
{
    public class CageFilterPagingRequest
    {
        public Guid? FarmId { get; set; }  // Lọc theo FarmId
        public string? PenCode { get; set; } // Lọc theo PenCode
        public string? Name { get; set; }  // Lọc theo Name của chuồng
        public string? SearchKey { get; set; }  // Trường tìm kiếm tổng hợp
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool? HasFarmingBatch { get; set; }

        public CageFilterModel MapToModel()
        {
            return new CageFilterModel
            {
                FarmId = FarmId,
                PenCode = PenCode,
                Name = Name,
                SearchKey = SearchKey,
                PageNumber = PageNumber,
                PageSize = PageSize,
                HasFarmingBatch = HasFarmingBatch
            };
        }
    }
   

}
