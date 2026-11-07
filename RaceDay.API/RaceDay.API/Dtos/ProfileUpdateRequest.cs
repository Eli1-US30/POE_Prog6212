using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Dtos
{
    public class ProfileUpdateRequest
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(300)]
        public string? ProfilePictureUrl { get; set; }
    }

}
