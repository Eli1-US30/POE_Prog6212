namespace RaceDay.API.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public User Participant { get; set; } = null!;
        public User Event { get; set; } = null!;
        public User Category { get; set; } = null!;
        public Result? Result { get; set; }
    }
}
