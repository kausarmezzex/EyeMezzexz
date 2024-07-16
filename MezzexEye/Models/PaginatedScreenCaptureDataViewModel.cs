using EyeMezzexz.Models;

namespace MezzexEye.Models
{
    public class PaginatedScreenCaptureDataViewModel
    {
        public List<ScreenCaptureDataViewModel> ScreenCaptures { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string MediaType { get; set; } // Add MediaType property
    }
}
