using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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

        [HttpPost("saveScreenCaptureData")]
        public IActionResult SaveScreenCaptureData([FromBody] UploadRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var taskTimer = _context.TaskTimers
                .Include(t => t.Task)
                .FirstOrDefault(t => t.Id == model.TaskTimerId);

            if (taskTimer == null)
            {
                return NotFound("TaskTimer not found");
            }

            var uploadedData = new UploadedData
            {
                ImageUrl = model.ImageUrl,
                SystemInfo = model.SystemInfo,
                ActivityLog = model.ActivityLog,
                Timestamp = model.Timestamp,
                Username = model.Username,
                SystemName = model.SystemName,
                TaskName = taskTimer.Task.Name, // Assign TaskName from TaskTimer
                TaskTimerId = model.TaskTimerId // Assign TaskTimerId
            };

            _context.UploadedData.Add(uploadedData);
            _context.SaveChanges();

            return Ok(new { Message = "Screen capture data uploaded successfully" });
        }

        [HttpGet("getScreenCaptureData")]
        public IActionResult GetScreenCaptureData()
        {
            var data = _context.UploadedData.Select(d => new
            {
                ImageUrl = d.ImageUrl,
                SystemInfo = d.SystemInfo,
                ActivityLog = d.ActivityLog,
                Timestamp = d.Timestamp,
                Username = d.Username,
                Id = d.Id,
                SystemName = d.SystemName,
                TaskName = d.TaskName // Include TaskName in the response
            }).ToList();

            return Ok(data);
        }

        [HttpPost("saveTaskTimer")]
        public IActionResult SaveTaskTimer([FromBody] TaskTimer model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            _context.TaskTimers.Add(model);
            _context.SaveChanges();
            if (model.Id == 0)
            {
                return StatusCode(500, "Failed to generate TaskTimer");
            }
            return Ok(new { Message = "Task timer data uploaded successfully", TaskTimeId = model.Id });
        }

        [HttpGet("getTaskTimers")]
        public IActionResult GetTaskTimers()
        {
            var today = DateTime.Today;

            var taskTimers = _context.TaskTimers
                .Include(t => t.Task)
                .Include(t => t.User)
                .Where(t => t.TaskStartTime.Date == today && t.TaskEndTime == null)
                .Select(t => new TaskTimerResponse
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    UserName = t.User.Username,
                    TaskId = t.TaskId,
                    TaskName = t.Task.Name,
                    TaskComment = t.TaskComment,
                    TaskStartTime = t.TaskStartTime,
                    TaskEndTime = t.TaskEndTime
                })
                .ToList();

            return Ok(taskTimers);
        }

        [HttpGet("getUserCompletedTasks")]
        public IActionResult GetUserCompletedTasks(int userId)
            {
            var today = DateTime.Today;

            var completedTaskTimers = _context.TaskTimers
                .Include(t => t.Task)
                .Include(t => t.User)
                .Where(t => t.UserId == userId && t.TaskStartTime.Date == today && t.TaskEndTime != null)
                .Select(t => new TaskTimerResponse
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    UserName = t.User.Username,
                    TaskId = t.TaskId,
                    TaskName = t.Task.Name,
                    TaskComment = t.TaskComment,
                    TaskStartTime = t.TaskStartTime,
                    TaskEndTime = t.TaskEndTime
                })
                .ToList();

            return Ok(completedTaskTimers);
        }

        [HttpGet("getTasks")]
        public IActionResult GetTasks()
        {
            var tasks = _context.Tasks.Select(t => new TaskModel
            {
                Id = t.Id,
                Name = t.Name
            }).ToList();

            return Ok(tasks);
        }

        [HttpPost("saveStaff")]
        public IActionResult SaveStaff([FromBody] Staff model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var user = _context.Users.Find(model.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            _context.Staffs.Add(model);
            _context.SaveChanges();

            if (model.Id == 0)
            {
                return StatusCode(500, "Failed to generate StaffId");
            }
            return Ok(new { message = "Staff data saved successfully", StaffId = model.Id });
        }

        [HttpPut("updateStaff")]
        public IActionResult UpdateStaff([FromBody] Staff model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var existingStaff = _context.Staffs.FirstOrDefault(s => s.Id == model.Id);
            if (existingStaff == null)
            {
                return NotFound("Staff not found");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == model.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            existingStaff.StaffInTime = model.StaffInTime;
            existingStaff.StaffOutTime = model.StaffOutTime;
            existingStaff.UserId = model.UserId;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the staff record: {ex.Message}");
            }

            return Ok(new { Message = "Staff data updated successfully", StaffId = existingStaff.Id });
        }

        [HttpGet("getStaff")]
        public IActionResult GetStaff()
        {
            var staff = _context.Staffs.Select(s => new
            {
                s.Id,
                s.StaffInTime,
                s.StaffOutTime
            }).ToList();

            return Ok(staff);
        }

        [HttpPut("updateTaskTimer")]
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
            taskTimer.TaskEndTime = model.TaskEndTime;

            _context.TaskTimers.Update(taskTimer);
            _context.SaveChanges();

            return Ok(new { Message = "Task timer updated successfully", TaskTimeId = taskTimer.Id });
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
        public int? TaskTimerId { get; set; } // Add TaskTimerId to UploadRequest
    }

    public class TaskTimerUploadRequest
    {
        public int UserId { get; set; }
        public int TaskId { get; set; }
        public string? TaskComment { get; set; }
        public DateTime TaskStartTime { get; set; }
        public DateTime? TaskEndTime { get; set; }
    }

    public class TaskTimerResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public string TaskComment { get; set; }
        public DateTime TaskStartTime { get; set; }
        public DateTime? TaskEndTime { get; set; }
    }

    public class UpdateTaskTimerRequest
    {
        public int Id { get; set; }
        public DateTime TaskEndTime { get; set; }
    }

    public class TaskModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
