namespace MezzexEye.Models
{
    public class TaskModelRequest
    {
        public string Name { get; set; }
        public int? ParentTaskId { get; set; }
        public int? CountryId { get; set; } // Add CountryId to TaskModelRequest
        public int? ComputerId { get; set; } // Add ComputerId to TaskModelRequest
        public bool? ComputerRequired { get; set; } // Add ComputerRequired to TaskModelRequest
        public string? TaskCreatedBy { get; set; }
        public string? TaskModifiedBy { get; set; }
    }
}
