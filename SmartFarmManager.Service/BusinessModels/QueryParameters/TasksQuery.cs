using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.QueryParameters
{
    public class TasksQuery
    {
        public string? Status { get; set; }
        public string? TaskType { get; set; }
        public string? SortBy { get; set; } = "DueDate"; // Mặc định sắp xếp theo DueDate
        public bool SortDescending { get; set; } = false; // Mặc định không sắp xếp giảm
        public int PageIndex { get; set; } = 1; // Mặc định trang đầu tiên
        public int PageSize { get; set; } = 10; // Mặc định 10 mục trên một trang
    }
}
