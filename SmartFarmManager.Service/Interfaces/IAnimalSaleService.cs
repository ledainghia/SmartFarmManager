using SmartFarmManager.Service.BusinessModels.AnimalSale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IAnimalSaleService
    {
        Task<bool> CreateAnimalSaleAsync(CreateAnimalSaleRequest request);
    }
}
