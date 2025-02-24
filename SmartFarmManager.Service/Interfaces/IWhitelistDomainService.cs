using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.WhiteListDomain;
using SmartFarmManager.Service.BusinessModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IWhitelistDomainService
    {
        Task<WhitelistDomain> AddDomainAsync(string domain);
        Task<PagedResult<WhiteListDomainItemModel>> GetAllDomainsAsync(int pageNumber, int pageSize);
       
    }
}
