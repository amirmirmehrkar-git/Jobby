using Domain.Enums;

namespace Application.CQRS.User_CQRS
{
    public class GetProfileDto
    {
        public string? Id { get; set; }
        public string? NationalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FatherName { get; set; }
        public string? Address { get; set; }
        public bool Active { get; set; }
        public string? ImageName { get; set; }
        public string? NeededJob { get; set; }
        public string? LinkedInUserName { get; set; }
        public string? LinkedInPassword { get; set; }
        public string? Indeed { get; set; }
        public string? StepStoneUserName { get; set; }
        public string? StepStonePassword { get; set; }
        public string? XingUserName { get; set; }
        public string? XingPassword { get; set; }
        public int? Age { get; set; }
        public string? AboutMe { get; set; }
        public bool? Married { get; set; }
        public double? WalletAmount { get; set; }
        public DateTime? CreateDate { get; set; }
        public bool Deleted { get; set; }
        public UserType? UserType { get; set; }
        public ApplyType? ApplyType { get; set; }
    }
}