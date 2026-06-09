namespace Application.CQRS.Ticket_CQRS
{
    public record CreateTicketMessageCommand
    {
        public Guid Id { get; set; }
        public string TextMessage { get; set; }
        public string FileName { get; set; }
        public Guid TicketId { get; set; }
        public string UserId { get; set; }
    }
}
