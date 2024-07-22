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
                CreatedOn = model.CreatedOn,
                Username = model.Username,
                SystemName = model.SystemName,
                TaskName = taskTimer.Task.Name, // Assign TaskName from TaskTimer
                TaskTimerId = model.TaskTimerId, // Assign TaskTimerId
                VideoUrl = model.VideoUrl
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
                Timestamp = d.CreatedOn,
                Username = d.Username,
                Id = d.Id,
                SystemName = d.SystemName,
                TaskName = d.TaskName, // Include TaskName in the response
                VideoUrl = d.VideoUrl
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
                    UserName = t.User.FirstName+" "+t.User.LastName,
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
                    UserName = t.User.FirstName +" "+ t.User.LastName,
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
            var tasks = _context.TaskNames.Select(t => new TaskNames
            {
                Id = t.Id,
                Name = t.Name
            }).ToList();

            return Ok(tasks);
        }

        [HttpPost("saveStaff")]
        public IActionResult SaveStaff([FromBody] StaffInOut model)
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

            _context.StaffInOut.Add(model);
            _context.SaveChanges();

            if (model.Id == 0)
            {
                return StatusCode(500, "Failed to generate StaffId");
            }
            return Ok(new { message = "Staff data saved successfully", StaffId = model.Id });
        }

        [HttpPost("updateStaff")]
        public IActionResult UpdateStaff([FromBody] StaffInOut model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var existingStaff = _context.StaffInOut.FirstOrDefault(s => s.Id == model.Id);
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
            var staff = _context.StaffInOut.Select(s => new
            {
                s.Id,
                s.StaffInTime,
                s.StaffOutTime
            }).ToList();

            return Ok(staff);
        }

        [HttpGet("getStaffInTime")]
        public IActionResult GetStaffInTime(int userId)
        {
            var today = DateTime.Today;

            var staffInOut = _context.StaffInOut
                .Where(s => s.UserId == userId && s.StaffInTime.Date == today)
                .OrderByDescending(s => s.StaffInTime)
                .FirstOrDefault();

            if (staffInOut == null)
            {
                return NotFound("Staff in time not found for the given user ID and today's date");
            }

            var response = new
            {
                StaffInTime = staffInOut.StaffInTime,
                StaffId = staffInOut.Id
            };

            return Ok(response);
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
            taskTimer.TaskEndTime = model.TaskEndTime;

            _context.TaskTimers.Update(taskTimer);
            _context.SaveChanges();

            return Ok(new { Message = "Task timer updated successfully", TaskTimeId = taskTimer.Id });
        }

        [HttpPost("createTask")]
        public IActionResult CreateTask([FromBody] TaskModelRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var task = new TaskNames
            {
                Name = model.Name,
                TaskCreatedBy = User.Identity.Name, // Assuming you have user identity setup
                TaskCreatedOn = DateTime.UtcNow
            };

            _context.TaskNames.Add(task);
            _context.SaveChanges();

            if (task.Id == 0)
            {
                return StatusCode(500, "Failed to create Task");
            }
            return Ok(new { Message = "Task created successfully", TaskId = task.Id });
        }

        [HttpGet("getTaskTimeId")]
        public IActionResult GetTaskTimeId(int taskId)
        {
            var taskTimer = _context.TaskTimers
                .Where(t => t.Id == taskId && t.TaskEndTime == null)
                .Select(t => new { t.Id })
                .FirstOrDefault();

            if (taskTimer == null)
            {
                return NotFound("TaskTimer not found");
            }

            return Ok(new { TaskTimeId = taskTimer.Id });
        }

        [HttpPut("updateTask")]
        public IActionResult UpdateTask([FromBody] TaskNames model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var existingTask = _context.TaskNames.FirstOrDefault(t => t.Id == model.Id);
            if (existingTask == null)
            {
                return NotFound("Task not found");
            }

            existingTask.Name = model.Name;
            existingTask.TaskModifiedBy = User.Identity.Name; // Assuming you have user identity setup
            existingTask.TaskModifiedOn = DateTime.UtcNow;

            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the task record: {ex.Message}");
            }

            return Ok(new { Message = "Task updated successfully", TaskId = existingTask.Id });
        }
    }

    public class UploadRequest
    {
        public string ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string Username { get; set; }
        public string SystemName { get; set; }
        public DateTime CreatedOn { get; set; }
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

    public class TaskModelRequest
    {
        public string Name { get; set; }
    }
}
