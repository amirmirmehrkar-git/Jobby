namespace Application.CQRS.Skill_CQRS
{
    public record CreateCommand
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        //public int? Value { get; set; }
    }
}
