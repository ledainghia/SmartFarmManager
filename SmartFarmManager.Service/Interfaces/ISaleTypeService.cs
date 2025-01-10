using SmartFarmManager.Service.BusinessModels.SaleType;
using SmartFarmManager.Service.BusinessModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface ISaleTypeService
    {
        Task<PagedResult<SaleTypeItemModel>> GetFilteredSaleTypesAsync(SaleTypeFilterModel filter);
    }
}
