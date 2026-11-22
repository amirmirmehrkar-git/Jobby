namespace Application.CQRS.Experience_CQRS
{
    public record CreateCommand
    {
        public Guid Id { get; set; }
        public string? Project { get; set; }
        public string? Company { get; set; }
        public string? Role { get; set; }
        public string? Activity { get; set; }
        public string? Benefit { get; set; }
    }
}
