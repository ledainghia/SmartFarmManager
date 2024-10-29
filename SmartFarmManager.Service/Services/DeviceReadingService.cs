﻿using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class DeviceReadingService : IDeviceReadingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeviceReadingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Implement methods for DeviceReading operations here
    }
}
