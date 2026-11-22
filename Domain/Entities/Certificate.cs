using DevExpress.Xpo;
using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Certificate : BaseEntity
    {
        public string? Title { get; set; }
        public string? Academy { get; set; }
        public string? Skill { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId"), Nullable(true)]
        public virtual ApplicationUser? User { get; set; }
    }
}