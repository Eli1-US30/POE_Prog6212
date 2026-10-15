namespace RaceDay.API.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int MinAge { get; set; }
        public int? MaxAge { get; set; }
        public int DistanceKm { get; set; }
        public Event Event { get; set; } = null!;
        public List<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
