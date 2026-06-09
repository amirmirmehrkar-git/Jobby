using Domain.Entities.Blog;
using Domain.Enums;

namespace Application.CQRS.Blog_CQRS
{
    public record CreateCommand
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? FileName { get; set; }
        public List<BlogDetailDto> Details { get; set; }
    }
}
