using Domain.Enums;

namespace Application.CQRS.Suggest_CQRS
{
    public class JobInfoDto
    {
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string? Location { get; set; }
        public string? Link { get; set; }
        public string? CoverLetter { get; set; }
        public JobType? Type { get; set; }
    }
}
