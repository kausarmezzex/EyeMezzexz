using Microsoft.EntityFrameworkCore;
using EyeMezzexz.Models;

namespace EyeMezzexz.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UploadedData> UploadedData { get; set; }
        public DbSet<TaskModel> Tasks { get; set; }
        public DbSet<TaskTimer> TaskTimers { get; set; }  // Add this line
    }
}
