using System.Collections.Generic;
using System.Threading.Tasks;
using EyeMezzexz.Controllers;
using EyeMezzexz.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MezzexEye.Services
{
    public class ShiftApiService : IShiftApiService
    {
        private readonly ShiftController _shiftController;
        private readonly ILogger<ShiftApiService> _logger;

        public ShiftApiService(ShiftController shiftController, ILogger<ShiftApiService> logger)
        {
            _shiftController = shiftController;
            _logger = logger;
        }

        // Fetch all shifts
        public async Task<List<Shift>> GetShiftsAsync()
        {
            var result = await _shiftController.GetShifts();

            if (result.Result is OkObjectResult okResult && okResult.Value is IEnumerable<Shift> shifts)
            {
                return new List<Shift>(shifts);
            }

            _logger.LogError("Failed to retrieve shifts.");
            return new List<Shift>();
        }

        // Fetch shift details by ID
        public async Task<Shift> GetShiftByIdAsync(int shiftId)
        {
            var result = await _shiftController.GetShiftById(shiftId);

            if (result.Result is OkObjectResult okResult && okResult.Value is Shift shift)
            {
                return shift;
            }

            _logger.LogError($"Failed to retrieve shift with ID {shiftId}.");
            return null;
        }

        // Fetch shifts by country ID
        public async Task<List<Shift>> GetShiftsByCountryAsync(int countryId)
        {
            var result = await _shiftController.GetShiftsByCountry(countryId);

            if (result.Result is OkObjectResult okResult && okResult.Value is IEnumerable<Shift> shifts)
            {
                return new List<Shift>(shifts);
            }

            _logger.LogError($"Failed to retrieve shifts for country ID {countryId}.");
            return new List<Shift>();
        }

        // Add a new shift with country ID
        public async Task<bool> AddShiftAsync(Shift shift, int countryId)
        {
            shift.CountryId = countryId; // Assign the country ID to the shift

            var result = await _shiftController.AddShift(shift);
            if (result.Result is CreatedAtActionResult)
            {
                _logger.LogInformation("Shift successfully created for country ID {countryId}.", countryId);
                return true;
            }

            _logger.LogError("Failed to create shift for country ID {countryId}.", countryId);
            return false;
        }

        // Update an existing shift
        public async Task<bool> UpdateShiftAsync(int shiftId, Shift updatedShift)
        {
            var result = await _shiftController.UpdateShift(shiftId, updatedShift);
            if (result is NoContentResult)
            {
                _logger.LogInformation("Shift successfully updated.");
                return true;
            }

            _logger.LogError("Failed to update shift.");
            return false;
        }

        // Delete a shift by ID
        public async Task<bool> DeleteShiftAsync(int shiftId)
        {
            var result = await _shiftController.DeleteShift(shiftId);
            if (result is NoContentResult)
            {
                _logger.LogInformation("Shift successfully deleted.");
                return true;
            }

            _logger.LogError($"Failed to delete shift with ID {shiftId}.");
            return false;
        }
    }
}
