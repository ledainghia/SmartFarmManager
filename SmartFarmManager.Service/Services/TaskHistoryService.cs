using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class TaskHistoryService : ITaskHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskHistoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Implement methods for TaskHistory operations here
    }
}
