namespace SmartFarmManager.API.Payloads.Requests.Task
{
    public class TaskFilterPagingRequest
    {

        public string? KeySearch { get; set; } // Tìm kiếm theo tên
        public string? Status { get; set; } // Lọc theo trạng thái
        public Guid? TaskTypeId { get;set; } // Lọc theo task type 
        public Guid? CageId { get; set; } // Lọc theo chuồng
        public Guid? AssignedToUserId { get; set; } // Lọc theo người được gán
        public DateTime? DueDateFrom { get; set; } // Lọc theo ngày hạn bắt đầu
        public DateTime? DueDateTo { get; set; } // Lọc theo ngày hạn kết thúc
        public int? PriorityNum { get; set; } // Lọc theo mức độ ưu tiên
        public int? Session {  get; set; }  // Lọc theo buổi trong ngày (1- sáng, 2- trưa, 3 chiều)
        public DateTime? CompletedAt { get; set; }
        public DateTime? CreatedAt { get; set; }

        public int PageNumber { get; set; } = 1; // Số trang
        public int PageSize { get; set; } = 10; // Số phần tử mỗi trang
    }
}
