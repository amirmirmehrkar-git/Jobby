namespace Application.CQRS.Suggest_CQRS
{
    public record SearchSentSuggestByUserIdCommand
    {
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string? Date { get; set; }
        public string? SuggestType { get; set; }
    }
}
