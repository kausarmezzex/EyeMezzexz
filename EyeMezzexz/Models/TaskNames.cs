using System;

namespace EyeMezzexz.Models
{
    public class TaskNames
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string? TaskCreatedBy { get; set; }
        public DateTime? TaskCreatedOn { get; set; }

        public string? TaskModifiedBy { get; set; }
        public DateTime? TaskModifiedOn { get; set; } // Make TaskModifiedOn nullable
    }
}
