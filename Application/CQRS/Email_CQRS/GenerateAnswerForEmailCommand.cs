namespace Application.CQRS.Email_CQRS
{
    public record GenerateAnswerForEmailCommand
    {
        public string? Message { get; set; }
    }
}
