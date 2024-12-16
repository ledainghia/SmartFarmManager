using AutoMapper;
using SmartFarmManager.DataAccessObject.Models;
using SmartFarmManager.Service.BusinessModels.Medication;
using SmartFarmManager.Service.BusinessModels.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Mapper
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper()
        {
            CreateMap<MedicationModel, Medication>().ReverseMap();
            CreateMap<TaskModel, DataAccessObject.Models.Task>().ReverseMap();


           
        }
    }
}
