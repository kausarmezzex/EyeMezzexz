using System.Security;

namespace EyeMezzexz.Models
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        public int PermissionId { get; set; }

        public ApplicationRole Role { get; set; }
        public PermissionName Permission { get; set; }
    }
}
