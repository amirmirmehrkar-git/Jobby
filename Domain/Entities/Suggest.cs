using DevExpress.Xpo;
using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Suggest : BaseEntity
    {
        public string? Title { get; set; }
        public string? Company { get; set; }
        public string? About { get; set; }
        public string? Description { get; set; }

        public string? Location { get; set; }
        public string? Link { get; set; }
        public string? CoverLetter { get; set; }
        public JobType? JobType { get; set; }
        public SuggestType? SuggestType { get; set; }
        public bool? IsLink { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId"), Nullable(true)]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<ResponseSuggest>? Responses { get; set; }
        public virtual ICollection<Question>? Questions { get; set; }
    }
}