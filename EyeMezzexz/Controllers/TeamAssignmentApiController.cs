using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using EyeMezzexz.Models;
using EyeMezzexz.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EyeMezzexz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamAssignmentApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TeamAssignmentApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TeamAssignmentApi/GetAssignmentData
        [HttpGet("GetAssignmentData")]
        public async Task<IActionResult> GetAssignmentData()
        {
            var teams = await _context.Teams.Where(t => !t.IsDeleted).ToListAsync();
            var users = await _context.Users.Where(u => u.Active).ToListAsync();
            var countries = await _context.Countries.ToListAsync();

            var model = new TeamAssignmentViewModel
            {
                Teams = teams.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name }).ToList(),
                Users = users.Select(u => new SelectListItem { Value = u.Id.ToString(), Text = $"{u.FirstName} {u.LastName}" }).ToList(),
                Countries = countries.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList()
            };

            return Ok(model);
        }

        // POST: api/TeamAssignmentApi/AssignUserToTeam
        [HttpPost("AssignUserToTeam")]
        public async Task<IActionResult> AssignUserToTeam([FromBody] TeamAssignmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var teamAssignment = new TeamAssignment
                {
                    TeamId = model.SelectedTeamId,
                    UserId = model.SelectedUserId,
                    CountryId = model.SelectedCountryId,
                    AssignedOn = DateTime.UtcNow
                };

                _context.TeamAssignments.Add(teamAssignment);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "User assigned to team successfully" });
            }

            return BadRequest(ModelState);
        }
    }
}
