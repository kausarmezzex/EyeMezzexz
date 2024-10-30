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
            return Ok(await _context.Shifts.Include(s => s.Country).ToListAsync());
        }

        // GET: api/Shift/ByCountry/{countryId}
        [HttpGet("ByCountry/{countryId}")]
        public async Task<ActionResult<IEnumerable<Shift>>> GetShiftsByCountry(int countryId)
        {
            var shifts = await _context.Shifts
                .Where(s => s.CountryId == countryId)
                .Include(s => s.Country)
                .ToListAsync();

            return Ok(shifts);
        }

        // POST: api/Shift
        [HttpPost]
        public async Task<ActionResult<Shift>> AddShift([FromBody] Shift shift)
        {
            if (shift == null)
                return BadRequest("Invalid shift data.");

            // Validate that the country exists
            var country = await _context.Countries.FindAsync(shift.CountryId);
            if (country == null)
                return BadRequest("Invalid country ID.");

            shift.CreatedOn = DateTime.Now;
            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShiftById), new { id = shift.ShiftId }, shift);
        }

        // GET: api/Shift/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Shift>> GetShiftById(int id)
        {
            var shift = await _context.Shifts.Include(s => s.Country).FirstOrDefaultAsync(s => s.ShiftId == id);
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

            // Validate the country ID, if provided
            if (updatedShift.CountryId != shift.CountryId)
            {
                var country = await _context.Countries.FindAsync(updatedShift.CountryId);
                if (country == null)
                    return BadRequest("Invalid country ID.");
                shift.CountryId = updatedShift.CountryId;
            }

            shift.ShiftName = updatedShift.ShiftName;
            shift.FromTime = updatedShift.FromTime;
            shift.ToTime = updatedShift.ToTime;
            shift.ModifiedBy = updatedShift.ModifiedBy;
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
