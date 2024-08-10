using Microsoft.AspNetCore.Mvc.Rendering;

namespace MezzexEye.Models
{
    public class CreateTeamViewModel
    {
        public string TeamName { get; set; }
        public int CountryId { get; set; }
        public List<SelectListItem> Countries { get; set; }
        public List<SelectListItem> Users { get; set; }
        public List<string> SelectedUserIds { get; set; }
    }

}
