using DevExpress.Xpo;
using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Skill : BaseEntity
    {
        public string? Title { get; set; }
        public string? Value { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId"), Nullable(true)]
        public virtual ApplicationUser? User { get; set; }
    }
}