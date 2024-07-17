using Microsoft.AspNetCore.Identity;

namespace EyeMezzexz.Models
{
    public class ApplicationRole : IdentityRole<int>
    {
        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
