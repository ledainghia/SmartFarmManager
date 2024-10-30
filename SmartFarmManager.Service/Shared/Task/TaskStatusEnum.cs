using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Shared.Task
{
    public static class TaskStatusEnum
    {
        public const string TO_DO = "To Do";
        public const string IN_PROGRESS = "In Progress";
        public const string VERIFYING = "Verifying";
        public const string DONE = "Done";
        public const string CANCEL = "Cancel";
    }
}
