using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Dtos
{
    public class ResultRequest
    {
        [Required] public int EnrolmentId { get; set; }
        [Required] public TimeSpan FinishTime { get; set; }
        [Range(1, int.MaxValue)] public int FinishPosition { get; set; }
    }
}
