using Domain.Enums;

namespace Application.CQRS.Blog_CQRS
{
    public record BlogDetailDto
    {
        public Guid? Id { get; set; }
        public InputType Key { get; set; }
        public string? Value { get; set; }
    }
}
