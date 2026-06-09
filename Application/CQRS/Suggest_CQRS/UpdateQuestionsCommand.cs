namespace Application.CQRS.Suggest_CQRS
{
    public record UpdateQuestionsCommand
    {
        public Guid? SuggestId { get; set; }
        public List<QuestionDto> Questions { get; set; }
    }

    public class QuestionDto
    {
        public Guid? Id { get; set; }
        public string? Answer { get; set; }
    }
}
