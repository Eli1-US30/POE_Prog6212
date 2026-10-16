using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Dtos
{
    public class CategoryRequest
    {
        [Required, StringLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }

        [Range(0.01, 9999.99)]
        public decimal? DistanceKm { get; set; }
    }
}