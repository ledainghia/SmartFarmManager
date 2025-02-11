using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using SmartFarmManager.Repository.Interfaces;
using SmartFarmManager.Service.Configuration;
using SmartFarmManager.Service.Interfaces;
using SmartFarmManager.Service.Shared;

namespace SmartFarmManager.API.BackgroundJobs.Jobs
{
    public class SymptomReportReminderJob : IJob
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        private readonly SystemConfiguration _systemConfig;

        public SymptomReportReminderJob(IUnitOfWork unitOfWork, INotificationService notificationService, IOptions<SystemConfiguration> systemConfig)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _systemConfig = systemConfig.Value;
        }

        public Task Execute(IJobExecutionContext context)
        {
            throw new NotImplementedException();
        }

        //public async Task Execute(IJobExecutionContext context)
        //{
        //    var symptoms = await _unitOfWork.MedicalSymptom.FindByCondition(s => s.Status == MedicalSymptomStatuseEnum.Pending).ToListAsync();

        //    foreach (var symptom in symptoms)
        //    {
        //        if (symptom.FirstReminderSentAt == null && symptom.CreateAt <= DateTime.Now.AddHours(-_systemConfig.FirstReminderTimeHours))
        //        {
        //            await _notificationService.SendReminderToDoctor(symptom.DoctorId, symptom);
        //            symptom.FirstReminderSentAt = DateTime.Now;
        //            await _unitOfWork.MedicalSymptom.UpdateAsync(symptom);
        //        }

        //        if (symptom.SecondReminderSentAt == null && symptom.FirstReminderSentAt.HasValue && symptom.FirstReminderSentAt.Value <= DateTime.Now.AddHours(-_systemConfig.SecondReminderTimeHours))
        //        {
        //            await _notificationService.SendReminderToDoctor(symptom.DoctorId, symptom);
        //            await _notificationService.SendReminderToAdmin(symptom.DoctorId, symptom);

        //            symptom.SecondReminderSentAt = DateTime.Now;
        //            await _unitOfWork.MedicalSymptom.UpdateAsync(symptom);
        //        }

        //        await _unitOfWork.CommitAsync();
        //    }
        //}
    }
}
