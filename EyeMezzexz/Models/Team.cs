namespace EyeMezzexz.Models
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public bool IsDeleted { get; set; }
        // Foreign key reference to Country
        public int? CountryId { get; set; }
        public Country? Country { get; set; } // Navigation property to Country
        public DateTime ModifyOn { get; set; }
        public string ModifyBy { get; set; }
    }
}
