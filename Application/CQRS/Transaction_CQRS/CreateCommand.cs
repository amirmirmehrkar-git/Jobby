namespace Application.CQRS.Transaction_CQRS
{
    public record CreateCommand
    {
        public double? Amount { get; set; }
    }
}
