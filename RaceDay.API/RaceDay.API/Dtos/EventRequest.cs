using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Dtos
{
    public class EventRequest
    {
        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required, StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 9999.99)]
        public decimal DistanceKm { get; set; }

        [Required]
        public string EventType { get; set; } = string.Empty; // run, walk or cycle

        [StringLength(300)]
        public string? BannerImageUrl { get; set; }
    }
}