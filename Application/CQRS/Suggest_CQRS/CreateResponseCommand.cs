namespace Application.CQRS.Suggest_CQRS
{
    public record CreateResponseCommand
    {
        public string? ResponseText { get; set; }
        public Guid? SuggestId { get; set; }
    }
}
