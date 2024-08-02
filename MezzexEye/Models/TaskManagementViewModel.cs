using EyeMezzexz.Models;

namespace MezzexEye.Models
{
    public class TaskManagementViewModel
    {
        public List<TaskNames> TaskTypes { get; set; }
        public List<TaskTimerResponse> ActiveTasks { get; set; }
        public List<TaskTimerResponse> CompletedTasks { get; set; }
    }
}
