using EyeMezzexz.Models;

namespace MezzexEye.ViewModel
{
    public class UserTaskViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool HasTasks { get; set; }
        public List<string> Computers { get; set; }
        public List<TaskDetail> Tasks { get; set; }
    }

}
