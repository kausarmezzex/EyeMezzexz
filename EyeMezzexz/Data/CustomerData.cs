using EyeMezzexz.Models;
using Microsoft.EntityFrameworkCore;

namespace EyeMezzexz.Data
{
    public class CustomerData : DbContext
    {
        public CustomerData(DbContextOptions<CustomerData> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }

}
