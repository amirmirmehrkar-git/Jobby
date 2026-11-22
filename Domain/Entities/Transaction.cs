using DevExpress.Xpo;
using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Transaction : BaseEntity
    {
        public PaymentState? State { get; set; }
        public DateTime? PayDate { get; set; }
        public string? FactorId { get; set; }
        public string? NumId { get; set; }
        public string? RefId { get; set; }
        public double? Rate { get; set; }
        public double? Amount { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId"), Nullable(true)]
        public virtual ApplicationUser? User { get; set; }
    }
}
