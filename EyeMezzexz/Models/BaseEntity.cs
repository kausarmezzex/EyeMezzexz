using System;
using System.ComponentModel.DataAnnotations;

namespace EyeMezzexz.Models
{
    public abstract class BaseEntity
    {
        [Required]
        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedOn { get; set; }
    }
}
