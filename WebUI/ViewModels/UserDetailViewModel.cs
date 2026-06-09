using Domain.Entities;

namespace WebAPI.ViewModels
{
    public class UserDetailViewModel
    {
        public List<Skill> Skills { get; set; }
        public List<Experience> Experiences { get; set; }
        public List<Education> Educations { get; set; }
        public List<Certificate> Certificates { get; set; }
    }
}
