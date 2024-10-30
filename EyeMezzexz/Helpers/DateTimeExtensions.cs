using System;

namespace EyeMezzexz.Helpers
{
    public static class DateTimeExtensions
    {
        // Extension method to get the start of the week
        public static DateTime StartOfWeek(this DateTime date, DayOfWeek startOfWeek)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
    }
}
