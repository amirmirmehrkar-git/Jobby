using DevExpress.Xpo;
using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Document : BaseEntity
    {
        public string? Title { get; set; }
        public string? FileName { get; set; }
        public bool? IsResume { get; set; }
        public FileType? FileType { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId"), Nullable(true)]
        public virtual ApplicationUser? User { get; set; }
    }
}