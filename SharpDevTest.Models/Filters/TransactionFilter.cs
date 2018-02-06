using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDevTest.Models.Filters
{
    public class TransactionFilter
    {
        public string RecipientName { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal? PwAmount { get; set; }
    }
}
