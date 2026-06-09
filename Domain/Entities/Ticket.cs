using Domain.Common;
using Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Ticket : BaseEntity
    {
        public string Title { get; set; }
        public string Number { get; set; }
        public Priority Priority { get; set; }
        public TicketStatus Status { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<TicketMessage>? Messages { get; set; }
    }
}