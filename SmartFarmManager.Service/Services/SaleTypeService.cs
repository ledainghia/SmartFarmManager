using Microsoft.EntityFrameworkCore;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.SaleType;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class SaleTypeService:ISaleTypeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SaleTypeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<SaleTypeItemModel>> GetFilteredSaleTypesAsync(SaleTypeFilterModel filter)
        {
            // Query cơ bản
            var query = _unitOfWork.SaleTypes.FindAll(false).AsQueryable();

            // Áp dụng các bộ lọc từ request
            if (!string.IsNullOrEmpty(filter.StageTypeName))
            {
                query = query.Where(s => s.StageTypeName.Contains(filter.StageTypeName));
            }

            // Tổng số phần tử
            var totalItems = await query.CountAsync();

            // Phân trang và lấy dữ liệu
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(s => new SaleTypeItemModel
                {
                    Id = s.Id,
                    StageTypeName = s.StageTypeName,
                    Description = s.Discription
                })
                .ToListAsync();

            var result = new Helpers.PaginatedList<SaleTypeItemModel>(items, totalItems, filter.PageNumber, filter.PageSize);
            return new PagedResult<SaleTypeItemModel>
            {
                Items = result.Items,
                TotalItems = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                HasNextPage = result.HasNextPage,
                HasPreviousPage = result.HasPreviousPage,
            };
        }

    }
}
