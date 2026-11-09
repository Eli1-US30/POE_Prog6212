using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    // What the API returns for an event
    public class EventSummary
    {
        public int EventId { get; set; }
        public int OrganiserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public decimal DistanceKm { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string? BannerImageUrl { get; set; }
        public int EnrolmentCount { get; set; }
    }

    // The create/edit form
    public class EventFormViewModel
    {
        public int EventId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required, DataType(DataType.Date), Display(Name = "Event date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EventDate { get; set; } = DateTime.Today.AddDays(30);

        [Required, StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 9999.99, ErrorMessage = "Distance must be between 0.01 and 9999.99 km.")]
        [Display(Name = "Distance (km)")]
        public decimal DistanceKm { get; set; }

        [Required, Display(Name = "Event type")]
        public string EventType { get; set; } = "run";

        public string? BannerImageUrl { get; set; } // filled in by the banner upload later
    }
    public class CategoryItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public decimal? DistanceKm { get; set; }
    }

    public class CategoryFormViewModel
    {
        public int EventId { get; set; }

        [Required, StringLength(50), Display(Name = "Category name")]
        public string CategoryName { get; set; } = string.Empty;

        [Range(0, 120), Display(Name = "Minimum age")]
        public int? MinAge { get; set; }

        [Range(0, 120), Display(Name = "Maximum age")]
        public int? MaxAge { get; set; }

        [Range(0.01, 9999.99), Display(Name = "Distance (km)")]
        public decimal? DistanceKm { get; set; }
    }

    public class CategoriesPageViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public List<CategoryItem> Categories { get; set; } = new();
        public CategoryFormViewModel Form { get; set; } = new();
    }

    public class EnrolmentItem
    {
        public int EnrolmentId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string ParticipantEmail { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; }
        public TimeSpan? FinishTime { get; set; }
        public int? FinishPosition { get; set; }
    }

    public class EnrolmentsPageViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public List<EnrolmentItem> Enrolments { get; set; } = new();
    }
    public class CreatedEvent
    {
        public int EventId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}