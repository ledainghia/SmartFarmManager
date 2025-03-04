using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.BusinessModels.Disease;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class DiseaseService : IDiseaseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DiseaseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<DiseaseModel>> GetPagedDiseasesAsync(string? name, int page, int pageSize)
        {
            var (items, totalCount) = await _unitOfWork.Diseases.GetPagedAsync(
                filter: d => string.IsNullOrEmpty(name) || d.Name.Contains(name),
                orderBy: q => q.OrderBy(d => d.Name),
                page: page,
                pageSize: pageSize
            );

            return new PagedResult<DiseaseModel>
            {
                Items = items.Select(d => new DiseaseModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                }),
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                HasNextPage = page < (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPreviousPage = page > 1
            };
        }
    }
}
