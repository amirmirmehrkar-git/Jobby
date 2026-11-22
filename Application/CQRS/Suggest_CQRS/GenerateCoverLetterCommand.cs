namespace Application.CQRS.Suggest_CQRS
{
    public record GenerateCoverLetterCommand
    {
        public List<JobInfoDto>? Jobs { get; set; }
    }
}
