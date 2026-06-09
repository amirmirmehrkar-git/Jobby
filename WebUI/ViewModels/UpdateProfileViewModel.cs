using Domain.Enums;

namespace WebAPI.ViewModels
{
    public class UpdateProfileViewModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AboutMe { get; set; }
        public string? NeededJob { get; set; }
        public string? ImageName { get; set; }
        public string? LinkedInUserName { get; set; }
        public string? LinkedInPassword { get; set; }
        public string? StepStoneUserName { get; set; }
        public string? StepStonePassword { get; set; }
        public string? XingUserName { get; set; }
        public string? XingPassword { get; set; }
        public ApplyType? ApplyType { get; set; }
    }
}
