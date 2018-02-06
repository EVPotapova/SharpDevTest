using System;

namespace SharpDevTest.Models.Filters
{
    public class TransactionFilter
    {
        public Guid? RecipientId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal? PwAmount { get; set; }
    }
}
