namespace Application.CQRS.Document_CQRS
{
    public record CreateCommand
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? FileName { get; set; }
    }
}
