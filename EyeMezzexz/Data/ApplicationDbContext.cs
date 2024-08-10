using EyeMezzexz.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EyeMezzexz.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public DbSet<StaffInOut> StaffInOut { get; set; }
        public DbSet<UploadedData> UploadedData { get; set; }
        public DbSet<TaskTimer> TaskTimers { get; set; }
        public DbSet<TaskNames> TaskNames { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<PermissionName> PermissionsName { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Computer> Computers { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamAssignment> TeamAssignments { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // This should call the base method to ensure Identity configuration is done

            // Configure RolePermission relationships
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            // Configure UserPermission relationships
            modelBuilder.Entity<UserPermission>()
                .HasKey(up => new { up.UserId, up.PermissionId });

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId);

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId);

            // Configure Staff relationships
            modelBuilder.Entity<StaffInOut>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<UploadedData>()
                .HasOne(ud => ud.TaskTimer)
                .WithMany()
                .HasForeignKey(ud => ud.TaskTimerId);

            modelBuilder.Entity<Country>().HasData(
            new Country { Id = 1, Name = "United Kingdom", Code = "UK" },
            new Country { Id = 2, Name = "India", Code = "IN" }
        );
            modelBuilder.Entity<TaskNames>()
            .HasOne(t => t.Country)
            .WithMany()
            .HasForeignKey(t => t.CountryId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskNames>()
           .HasOne(t => t.ParentTask)
           .WithMany(t => t.SubTasks)
           .HasForeignKey(t => t.ParentTaskId);


        }
    }
}
