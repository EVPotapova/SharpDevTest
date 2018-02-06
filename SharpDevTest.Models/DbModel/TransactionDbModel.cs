using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SharpDevTest.Models.DbModel
{
    public class TransactionDbModel
    {
        public Guid Id { get; set; }
        public string SenderId { get; set; }
        public string RecipientId { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public decimal PwAmount { get; set; }

        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; }
        [ForeignKey("RecipientId")]
        public virtual ApplicationUser Recipient { get; set; }
    }
}
