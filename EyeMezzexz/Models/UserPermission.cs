namespace EyeMezzexz.Models
{
    public class UserPermission
    {
        public string UserId { get; set; }
        public int PermissionId { get; set; }

        public ApplicationUser User { get; set; }
        public Permission Permission { get; set; }
    }
}
