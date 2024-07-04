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
                id = d.Id,
                SystemName = d.SystemName
            }).ToList();

            return Ok(data);
        }

        [HttpPost("saveTaskTimer")]
        public IActionResult SaveTaskTimer([FromBody] TaskTimerUploadRequest model)
        {
            if (model == null)
            {
                return BadRequest("Model is null");
            }

            var taskTimer = new TaskTimer
            {
                UserId = model.UserId,
                TaskId = model.TaskId,  // Use TaskId from the model
                TaskComment = model.TaskComment
            };

            _context.TaskTimers.Add(taskTimer);
            _context.SaveChanges();

            return Ok(new { Message = "Task timer data uploaded successfully" });
        }

        [HttpGet("getTaskTimers")]
        public IActionResult GetTaskTimers()
        {
            var taskTimers = _context.TaskTimers.Select(t => new TaskTimerResponse
            {
                Id = t.Id,
                UserId = t.UserId,
                TaskId = t.TaskId,
                TaskName = t.Task.Name,
                TaskComment = t.TaskComment
            }).ToList();

            return Ok(taskTimers);
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
                // Print error or handle the case where Id is not set
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

            // Retrieve the existing staff member
            var existingStaff = _context.Staffs.FirstOrDefault(s => s.Id == model.Id);
            if (existingStaff == null)
            {
                return NotFound("Staff not found");
            }

            // Check if the user exists
            var user = _context.Users.FirstOrDefault(u => u.Id == model.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Update properties
            existingStaff.StaffInTime = model.StaffInTime;
            existingStaff.StaffOutTime = model.StaffOutTime;
            existingStaff.UserId = model.UserId;

            // Save changes
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while updating the staff record: {ex.Message}");
            }

            return Ok(new
            {
                Message = "Staff data updated successfully",
                StaffId = existingStaff.Id
            });
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

            // No calculation needed as TotalWorkingTime is removed

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

    public class TaskTimerUploadRequest
    {
        public int UserId { get; set; }
        public int TaskId { get; set; }  // Foreign key reference to TaskModel
        public string TaskComment { get; set; }
    }

    public class TaskTimerResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }
        public string TaskName { get; set; }  // Include Task name for better understanding
        public string TaskComment { get; set; }
    }

    public class UpdateTaskTimerRequest
    {
        public int Id { get; set; }
        public DateTime StaffOutTime { get; set; }
    }
}
