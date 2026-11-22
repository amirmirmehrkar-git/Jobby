using Domain.Common;
using Domain.Enums;

namespace Domain.Entities.Blog
{
    public class Blog : BaseEntity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? FileName { get; set; }
        public FileType? FileType { get; set; }

        public virtual ICollection<BlogDetail> Details { get; set; }
    }
}