using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EyeMezzexz.Models;
using EyeMezzexz.Data;
using Microsoft.EntityFrameworkCore;

namespace EyeMezzexz.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShiftController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Shift
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Shift>>> GetShifts()
        {
            return Ok(await _context.Shifts.ToListAsync());
        }

        // POST: api/Shift
        [HttpPost]
        public async Task<ActionResult<Shift>> AddShift([FromBody] Shift shift)
        {
            if (shift == null)
                return BadRequest("Invalid shift data.");

            shift.CreatedOn = DateTime.Now;
            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShiftById), new { id = shift.ShiftId }, shift);
        }

        // GET: api/Shift/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Shift>> GetShiftById(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
                return NotFound();

            return Ok(shift);
        }

        // PUT: api/Shift/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateShift(int id, [FromBody] Shift updatedShift)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
                return NotFound();

            shift.ShiftName = updatedShift.ShiftName;
            shift.FromTime = updatedShift.FromTime;
            shift.ToTime = updatedShift.ToTime;
            shift.ModifiedBy = updatedShift.ModifiedBy; // Set ModifiedBy as needed
            shift.ModifiedOn = DateTime.Now;

            _context.Shifts.Update(shift);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Shift/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteShift(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
                return NotFound();

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
