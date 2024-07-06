namespace EyeMezzexz.Models
{
    public class ComputeSaltedHashRequest
    {
        public int Salt { get; set; }
        public string ClearPassword { get; set; }
    }
}
