﻿using System;
using System.Collections.Generic;

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

        // Foreign key reference to Country
        public int? CountryId { get; set; }
        public Country? Country { get; set; } // Navigation property to Country

        // Foreign key reference to Computer
        public int? ComputerId { get; set; }
        public Computer? Computer { get; set; } // Navigation property to Computer

        // Hierarchical structure for sub-tasks
        public int? ParentTaskId { get; set; }
        public TaskNames? ParentTask { get; set; }
        public ICollection<TaskNames> SubTasks { get; set; } = new List<TaskNames>();

        // Additional property for UK-based tasks
        public bool? ComputerRequired { get; set; } // Nullable to allow non-UK tasks to ignore it
    }
}
