using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? NationalCode { get; set; }
        public string? FatherName { get; set; }
        public string? Address { get; set; }
        public bool Active { get; set; }
        public string? ImageName { get; set; }
        public string? Linkdin { get; set; }
        public string? Indeed { get; set; }
        public string? Stepstone { get; set; }
        public string? Xing { get; set; }
        public int? Age { get; set; }
        public string? AboutMe { get; set; }
        public bool? Married { get; set; }
        public DateTime? CreateDate { get; set; }
        public bool Deleted { get; set; }
        public UserType? UserType { get; set; }
        public ApplyType? ApplyType { get; set; }
    }
}