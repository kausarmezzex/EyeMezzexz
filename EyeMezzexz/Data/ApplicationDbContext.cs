using EyeMezzexz.Models;
using Microsoft.EntityFrameworkCore;

namespace EyeMezzexz.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<UploadedData> UploadedData { get; set; }
        public DbSet<TaskTimer> TaskTimers { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<Demo> demos { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Staff>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId);

            modelBuilder.Entity<UploadedData>()
                .HasOne(ud => ud.TaskTimer)
                .WithMany()
                .HasForeignKey(ud => ud.TaskTimerId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
