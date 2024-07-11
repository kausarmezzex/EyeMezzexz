using System.Security;

namespace EyeMezzexz.Models
{
    public class RolePermission
    {
        public string RoleId { get; set; }
        public int PermissionId { get; set; }

        public ApplicationRole Role { get; set; }
        public Permission Permission { get; set; }
    }
}
