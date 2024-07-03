using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using System;
using System.Linq;

namespace EyeMezzexz.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DataController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("upload")]
        public IActionResult UploadData([FromBody] UploadRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var uploadedData = new UploadedData
            {
                ImageUrl = model.ImageUrl,
                SystemInfo = model.SystemInfo,
                ActivityLog = model.ActivityLog,
                Timestamp = model.Timestamp,
                Username = model.Username,
                SystemName = model.SystemName
            };

            _context.UploadedData.Add(uploadedData);
            _context.SaveChanges();

            return Ok(new { Message = "Data uploaded successfully" });
        }

        [HttpGet("getdata")]
        public IActionResult GetData()
        {
            var data = _context.UploadedData.Select(d => new
            {
                ImageUrl = d.ImageUrl,
                SystemInfo = d.SystemInfo,
                ActivityLog = d.ActivityLog,
                Timestamp = d.Timestamp,
                Username = d.Username,
                id = d.Id,
                SystemName = d.SystemName
            }).ToList();

            return Ok(data);
        }

        [HttpGet("gettasks")]
        public IActionResult GetTasks()
        {
            var tasks = _context.Tasks.Select(t => new TaskModel
            {
                Id = t.Id,
                Name = t.Name
            }).ToList();

            return Ok(tasks);
        }

        [HttpPost("uploadTaskTimer")]
        public IActionResult UploadTaskTimer([FromBody] TaskTimerUploadRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var taskTimer = new TaskTimer
            {
                UserId = model.UserId,
                TaskId = model.TaskId,  // Use TaskId from the model
                TaskComment = model.TaskComment,
                StaffInTime = model.StaffInTime,
                StaffOutTime = model.StaffOutTime
            };

            _context.TaskTimers.Add(taskTimer);
            _context.SaveChanges();

            return Ok(new { Message = "Task timer data uploaded successfully" });
        }

        [HttpGet("gettasktimers")]
        public IActionResult GetTaskTimers()
        {
            var taskTimers = _context.TaskTimers.Select(t => new TaskTimerResponse
            {
                Id = t.Id,
                UserId = t.UserId,
                TaskId = t.TaskId,
                TaskName = t.Task.Name,
                TaskComment = t.TaskComment,
                StaffInTime = t.StaffInTime,
                StaffOutTime = t.StaffOutTime,
                TotalWorkingTime = t.TotalWorkingTime
            }).ToList();

            return Ok(taskTimers);
        }

        [HttpPost("updateTaskTimer")]
        public IActionResult UpdateTaskTimer([FromBody] UpdateTaskTimerRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var taskTimer = _context.TaskTimers.FirstOrDefault(t => t.Id == model.Id);
            if (taskTimer == null)
            {
                return NotFound("TaskTimer not found");
            }

            taskTimer.StaffOutTime = model.StaffOutTime;
            taskTimer.TotalWorkingTime = taskTimer.StaffOutTime - taskTimer.StaffInTime;

            _context.TaskTimers.Update(taskTimer);
            _context.SaveChanges();

            return Ok(new { Message = "Task timer updated successfully" });
        }
    }

    public class UploadRequest
    {
        public string ImageUrl { get; set; }
        public string SystemInfo { get; set; }
        public string ActivityLog { get; set; }
        public string Username { get; set; }
        public string SystemName { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class TaskModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TaskTimerUploadRequest
    {
        public int UserId { get; set; }
        public int TaskId { get; set; }  // Foreign key reference to TaskModel
        public string TaskComment { get; set; }
        public DateTime StaffInTime { get; set; }
        public DateTime StaffOutTime { get; set; }
    }

    public class TaskTimerResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }
        public string TaskName { get; set; }  // Include Task name for better understanding
        public string TaskComment { get; set; }
        public DateTime StaffInTime { get; set; }
        public DateTime StaffOutTime { get; set; }
        public TimeSpan TotalWorkingTime { get; set; }
    }

    public class UpdateTaskTimerRequest
    {
        public int Id { get; set; }
        public DateTime StaffOutTime { get; set; }
    }
}
