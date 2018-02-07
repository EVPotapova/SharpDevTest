using System;

namespace SharpDevTest.Models.Response
{
    public class TransactionGetModel
    {
        public Guid Id { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal PwAmount { get; set; }

        public virtual UserGetModel Sender { get; set; }
        public virtual UserGetModel Recipient { get; set; }
    }
}
