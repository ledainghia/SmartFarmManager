using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.Service.Services
{
    public class LivestockSaleService : ILivestockSaleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LivestockSaleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Implement methods for LivestockSale operations here
    }
}
