using DevExpress.Xpo;
using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Experience : BaseEntity
    {
        public string? Project { get; set; }
        public string? Company { get; set; }
        public string? Role { get; set; }
        public string? Activity { get; set; }
        public string? Benefit { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId"), Nullable(true)]
        public virtual ApplicationUser? User { get; set; }
    }
}