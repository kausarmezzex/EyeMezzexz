using System;
using System.ComponentModel.DataAnnotations;

namespace EyeMezzexz.Models
{
    public class ShiftAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int ShiftId { get; set; } // Foreign key to Shift
        public Shift Shift { get; set; } // Navigation property

        [Required]
        public int UserId { get; set; } // Foreign key to ApplicationUser
        public ApplicationUser User { get; set; } // Navigation property

        public DateTime AssignedOn { get; set; } = DateTime.Now;

        // New audit properties
        [Required]
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }
}
