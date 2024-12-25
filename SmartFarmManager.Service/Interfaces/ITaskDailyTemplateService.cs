using SmartFarmManager.Service.BusinessModels.GrowthStageTemplate;
using SmartFarmManager.Service.BusinessModels;
using SmartFarmManager.Service.BusinessModels.TaskDailyTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Interfaces
{
    public interface ITaskDailyTemplateService
    {
        Task<bool> CreateTaskDailyTemplateAsync(CreateTaskDailyTemplateModel model);
        Task<bool> CreateTaskDailyTemplatesAsync(List<CreateTaskDailyTemplateModel> models);
        Task<bool> UpdateTaskDailyTemplateAsync(UpdateTaskDailyTemplateModel model);
        Task<bool> DeleteTaskDailyTemplateAsync(Guid id);
        Task<PagedResult<TaskDailyTemplateItemModel>> GetTaskDailyTemplatesAsync(TaskDailyTemplateFilterModel filter);
        Task<TaskDailyTemplateDetailModel?> GetTaskDailyTemplateDetailAsync(Guid id);
    }
}
