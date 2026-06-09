using DevExpress.Xpo;
using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Question : BaseEntity
    {
        public string? Title { get; set; }
        public string? Answer { get; set; }
        public string? InputType { get; set; }
        public string? Selector { get; set; }
        public string? Options { get; set; }

        public Guid? SuggestId { get; set; }
        [ForeignKey("SuggestId"), Nullable(true)]
        public virtual Suggest? Suggest { get; set; }
    }
}