using Domain.Enums;

namespace Application.CQRS.Ticket_CQRS
{
    public record CreateCommand
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public string? Title { get; set; }
        public string? TextMessage { get; set; }
        public string? FileName { get; set; }
        public Priority Priority { get; set; }
    }
}
