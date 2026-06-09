using Domain.Enums;

namespace Application.CQRS.Suggest_CQRS
{
    public record SendApplicationCommand
    {
        public Guid? Id { get; set; }
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string? Location { get; set; }
        public string? Link { get; set; }
        public string? CoverLetter { get; set; }
        public JobType? JobType { get; set; }
    }
}
