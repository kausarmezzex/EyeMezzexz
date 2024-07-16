namespace EyeMezzexz.Models
{
    public class UploadedData
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string SystemInfo { get; set; }
        public string ActivityLog { get; set; }
        public DateTime Timestamp { get; set; }
        public string Username { get; set; } // Added to store user details
        public string SystemName { get; set; } // Add SystemName property
        public string? TaskName { get; set; } // Add TaskName property
        public int? TaskTimerId { get; set; } // Foreign key reference to TaskTimer
        public TaskTimer? TaskTimer { get; set; } // Navigation property
    }
}
