using DevExpress.Xpo;
using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ResponseSuggest : BaseEntity
    {
        public string? ResponseText { get; set; }
        public string? AiResult { get; set; }

        public Guid? SuggestId { get; set; }
        [ForeignKey("SuggestId"), Nullable(true)]
        public virtual Suggest? Suggest { get; set; }
    }
}