using Microsoft.EntityFrameworkCore;
using System;
namespace EyeMezzexz.Data
{
    public static class DataContextExtensions
    {
        public static DateTime GetDatabaseServerTime(this ApplicationDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            try
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT GETDATE()";
                    var result = command.ExecuteScalar();
                    return Convert.ToDateTime(result);
                }
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
