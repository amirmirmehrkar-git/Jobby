using Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class TicketMessage : BaseEntity
    {
        public string TextMessage { get; set; }
        public string FileName { get; set; }

        public Guid TicketId { get; set; }
        [ForeignKey("TicketId")]
        public virtual Ticket Ticket { get; set; }

        public string FromId { get; set; }
        [ForeignKey("FromId")]
        public virtual ApplicationUser From { get; set; }
    }
}