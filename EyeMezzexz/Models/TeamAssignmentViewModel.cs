using Microsoft.AspNetCore.Mvc.Rendering;

namespace EyeMezzexz.Models
{
    public class TeamAssignmentViewModel
    {
        public int SelectedTeamId { get; set; }
        public int SelectedUserId { get; set; }
        public int SelectedCountryId { get; set; }

        public List<SelectListItem>? Teams { get; set; }
        public List<SelectListItem>? Users { get; set; }
        public List<SelectListItem>? Countries { get; set; }
    }
}
