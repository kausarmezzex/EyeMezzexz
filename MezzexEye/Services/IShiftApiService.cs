using System.Collections.Generic;
using System.Threading.Tasks;
using EyeMezzexz.Models;

namespace MezzexEye.Services
{
    public interface IShiftApiService
    {
        Task<List<Shift>> GetShiftsAsync(); // Fetch all shifts
        Task<Shift> GetShiftByIdAsync(int shiftId); // Fetch shift by ID
        Task<List<Shift>> GetShiftsByCountryAsync(int countryId); // Fetch shifts by country ID
        Task<bool> AddShiftAsync(Shift shift, int countryId); // Create a new shift with country
        Task<bool> UpdateShiftAsync(int shiftId, Shift updatedShift); // Update an existing shift
        Task<bool> DeleteShiftAsync(int shiftId); // Delete a shift
    }
}
