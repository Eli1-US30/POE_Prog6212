namespace RaceDay.API.Models
{
    public class Event
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public decimal DistanceKm { get; set; }
        public string? BannerImageUrl { get; set; }
        public User Organiser { get; set; } = null!;
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    }
}
