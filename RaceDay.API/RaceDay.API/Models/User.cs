namespace RaceDay.API.Models
{
    public class User
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;
        public string FullName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Event> OrganisedEvents { get; set; } = new List<Event>();
        public List<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    }
}
