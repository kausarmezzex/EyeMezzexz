using System;
using System.ComponentModel.DataAnnotations;

namespace EyeMezzexz.Models
{
    public class Shift
    {
        [Key]
        public int ShiftId { get; set; }

        [Required]
        [StringLength(50)]
        public string ShiftName { get; set; }

        [Required]
        public TimeSpan FromTime { get; set; }

        [Required]
        public TimeSpan ToTime { get; set; }

        [Required]
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }
}
