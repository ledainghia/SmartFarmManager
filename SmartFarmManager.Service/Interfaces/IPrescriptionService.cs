using SmartFarmManager.DataAccessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface IPrescriptionService
    {
        Task<Prescription> CreatePrescriptionAsync(Prescription prescription);
    }
}
