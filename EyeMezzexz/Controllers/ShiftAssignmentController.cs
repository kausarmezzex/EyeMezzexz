using Microsoft.AspNetCore.Mvc;
using EyeMezzexz.Data;
using EyeMezzexz.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EyeMezzexz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftAssignmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShiftAssignmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ShiftAssignment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShiftAssignment>>> GetShiftAssignments()
        {
            var assignments = await _context.ShiftAssignments
                .Include(sa => sa.Shift)
                .Include(sa => sa.User)
                .ToListAsync();

            return Ok(assignments);
        }

        // GET: api/ShiftAssignment/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftAssignment>> GetShiftAssignmentById(int id)
        {
            var assignment = await _context.ShiftAssignments
                .Include(sa => sa.Shift)
                .Include(sa => sa.User)
                .FirstOrDefaultAsync(sa => sa.AssignmentId == id);

            if (assignment == null)
                return NotFound();

            return Ok(assignment);
        }

        // POST: api/ShiftAssignment
        [HttpPost]
        public async Task<ActionResult<ShiftAssignment>> AssignShift([FromBody] ShiftAssignment assignment)
        {
            if (assignment == null)
                return BadRequest("Invalid assignment data.");

            // Validate shift and user existence
            var shiftExists = await _context.Shifts.AnyAsync(s => s.ShiftId == assignment.ShiftId);
            var userExists = await _context.Users.AnyAsync(u => u.Id == assignment.UserId);

            if (!shiftExists || !userExists)
                return BadRequest("Invalid shift or user ID.");

            // Set audit properties
            assignment.CreatedOn = DateTime.Now;

            _context.ShiftAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShiftAssignmentById), new { id = assignment.AssignmentId }, assignment);
        }

        // PUT: api/ShiftAssignment/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateShiftAssignment(int id, [FromBody] ShiftAssignment updatedAssignment)
        {
            var assignment = await _context.ShiftAssignments.FindAsync(id);
            if (assignment == null)
                return NotFound();

            // Update fields
            assignment.ShiftId = updatedAssignment.ShiftId;
            assignment.UserId = updatedAssignment.UserId;
            assignment.AssignedOn = updatedAssignment.AssignedOn;

            // Set audit properties
            assignment.ModifiedBy = "System"; // Set based on the logged-in user if available
            assignment.ModifiedOn = DateTime.Now;

            _context.ShiftAssignments.Update(assignment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/ShiftAssignment/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteShiftAssignment(int id)
        {
            var assignment = await _context.ShiftAssignments.FindAsync(id);
            if (assignment == null)
                return NotFound();

            _context.ShiftAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
