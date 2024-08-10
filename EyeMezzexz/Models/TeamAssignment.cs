namespace EyeMezzexz.Models
{
    public class TeamAssignment
    {
        public int Id { get; set; }
        public int TeamId { get; set; }
        public int UserId { get; set; }
        public int CountryId { get; set; }
        public DateTime AssignedOn { get; set; }

        public Team Team { get; set; }
        public ApplicationUser User { get; set; }
        public Country Country { get; set; }
    }
}
