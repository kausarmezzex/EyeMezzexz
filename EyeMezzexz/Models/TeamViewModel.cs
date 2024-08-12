namespace EyeMezzexz.Models
{
    public class TeamViewModel
    {
        public int Id { get; set; }                  // Unique identifier for the team
        public string Name { get; set; }             // Name of the team
        public int? CountryId { get; set; }          // Foreign key for the associated country
        public string? CountryName { get; set; }      // Name of the associated country (for display purposes)
        public DateTime CreatedOn { get; set; }      // Date and time when the team was created
        public string? CreatedBy { get; set; }        // User who created the team
        public DateTime ModifyOn { get; set; }      // Date and time when the team was last modified (optional)
        public string? ModifyBy { get; set; }         // User who last modified the team (optional)
    }
}
