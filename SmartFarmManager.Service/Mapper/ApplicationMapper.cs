using AutoMapper;
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
            CreateMap<TaskModel, Task>().ReverseMap();

           
        }
    }
}
