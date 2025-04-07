using SmartFarmManager.Service.BusinessModels.StockLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IStockLogService
    {
        Task<bool> AddStockAsync(StockLogRequestModel stockLogRequest);
    }
}
