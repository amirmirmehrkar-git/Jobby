using Domain.Enums;

namespace WebAPI.ViewModels
{
    public class JobInfoViewModel
    {
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string? Location { get; set; }
        public string? Link { get; set; }
        public string? CoverLetter { get; set; }
        public JobType? JobType { get; set; }
    }
}
