namespace Application.CQRS.Education_CQRS
{
    public record CreateCommand
    {
        public Guid Id { get; set; }
        public string? Academy { get; set; }
        public string? Degree { get; set; }
        public double? Score { get; set; }
    }
}
