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
}