namespace RaceDay.MVC.Models
{
    public class EventFilter
    {
        public string? Type { get; set; }
        public string? Search { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class EventsPageViewModel
    {
        public List<EventSummary> Events { get; set; } = new();
        public EventFilter Filter { get; set; } = new();
    }

    public class MyEnrolmentItem
    {
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
    }

    public class EventDetailsViewModel
    {
        public EventSummary Event { get; set; } = new();
        public List<CategoryItem> Categories { get; set; } = new();
        public MyEnrolmentItem? MyEnrolment { get; set; }
    }

    public class MyResultItem
    {
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public TimeSpan FinishTime { get; set; }
        public int FinishPosition { get; set; }
        public int TotalFinishers { get; set; }
    }

    public class ProfileViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
    }
}