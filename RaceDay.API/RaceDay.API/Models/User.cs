namespace RaceDay.API.Models
{
    public class User
    {
        public int UserId { get; set; }
        public Role RoleId { get; set; } = null!;
        public Role Role { get; set; } = null!;
        public string FullName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfilePictureURL { get; set; }
        public DateTime DateOnly { get; set; }
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    }
}
