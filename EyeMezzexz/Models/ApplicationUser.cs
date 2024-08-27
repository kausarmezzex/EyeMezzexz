using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace EyeMezzexz.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public bool Active { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public DateTime? LastLogoutTime { get; set; }

        // New properties
        public string? CountryName { get; set; }
        public ICollection<TaskTimer> TaskTimers { get; set; }
        public ICollection<StaffInOut> StaffInOuts { get; set; }
        public ICollection<UserPermission> UserPermissions { get; set; }
    }
}
