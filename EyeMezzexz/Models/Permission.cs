namespace EyeMezzexz.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<UserPermission> UserPermissions { get; set; }
        public ICollection<RolePermission> RolePermissions { get; set; }
    }
}
