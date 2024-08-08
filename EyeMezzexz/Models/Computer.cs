namespace EyeMezzexz.Models
{
    public class Computer
    {
        public int Id { get; set; }
        public string Name { get; set; } // Only include the name of the computer

        // New fields added
        public DateTime CreatedOn { get; set; } // Timestamp when the record was created
        public string? CreatedBy { get; set; } // User who created the record
        public DateTime? ModifyOn { get; set; } // Timestamp when the record was last modified
        public string? ModifyBy { get; set; } // User who last modified the record
        public bool IsDeleted { get; set; } // Flag to indicate if the record is deleted
    }
}
