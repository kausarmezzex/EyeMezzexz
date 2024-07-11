using Microsoft.AspNetCore.Identity;

namespace EyeMezzexz.Models
{
    public class ApplicationRole : IdentityRole
    {
        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
