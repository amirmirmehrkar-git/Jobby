namespace Application.CQRS.Certificate_CQRS
{
    public record CreateCommand
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Academy { get; set; }
        public string? Skill { get; set; }
    }
}
