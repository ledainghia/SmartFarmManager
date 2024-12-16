using SmartFarmManager.Service.BusinessModels.Cages;

namespace SmartFarmManager.API.Payloads.Requests.Cages
{
    public class CageFilterPagingRequest
    {
        public Guid? FarmId { get; set; } // Lọc theo farm
        public string? AnimalType { get; set; } // Lọc theo loại động vật
        public string? Name { get; set; } // Tìm kiếm theo tên
        public bool? BoardStatus { get; set; } // Lọc theo trạng thái board
        public int PageNumber { get; set; } = 1; // Số trang (mặc định là 1)
        public int PageSize { get; set; } = 10; // Số item trên mỗi trang (mặc định là 10)


        public CageFilterModel MapToModel()
        {
            return new CageFilterModel()
            {
                FarmId = this.FarmId,
                AnimalType = this.AnimalType,
                Name = this.Name,
                BoardStatus = this.BoardStatus,
                PageNumber = this.PageNumber,
                PageSize = this.PageSize
            };
        }
    }
}
