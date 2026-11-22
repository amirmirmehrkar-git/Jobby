using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Blog
{
    public class BlogDetail : BaseEntity
    {
        public InputType Key { get; set; }
        public string Value { get; set; }

        public Guid BlogId { get; set; }
        [ForeignKey("BlogId")]
        public virtual Blog Blog { get; set; }
    }
}