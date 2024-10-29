using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.Interfaces;

namespace SmartFarmManager.Service.Services
{
    public class LivestockExpenseService : ILivestockExpenseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LivestockExpenseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Implement methods for LivestockExpense operations here
    }
}
