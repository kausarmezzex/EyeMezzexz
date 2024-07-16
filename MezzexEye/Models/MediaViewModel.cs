namespace MezzexEye.Models
{
    public class MediaViewModel
    {
        public string MediaUrl { get; set; }
        public string Username { get; set; }
        public DateTime Timestamp { get; set; }
        public string TaskName { get; set; }
        public string MediaType { get; set; } // "Image" or "Video"
    }
}
