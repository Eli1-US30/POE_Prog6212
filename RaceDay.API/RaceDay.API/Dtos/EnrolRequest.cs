using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Dtos
{
    public class EnrolRequest
    {
        [Required] public int EventId { get; set; }
        [Required] public int CategoryId { get; set; }
    }

}
