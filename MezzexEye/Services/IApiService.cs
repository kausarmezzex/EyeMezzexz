using EyeMezzexz.Models;

namespace MezzexEye.Services
{
    public interface IApiService
    {
        Task<ApplicationUser> GetUserByEmailAsync(string email);
    }
}
