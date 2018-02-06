using System;
using System.ComponentModel.DataAnnotations;

namespace SharpDevTest.Models.Request
{
    public class TransactionPostModel
    {
        [Required]
        public Guid SenderId { get; set; }
        [Required]
        public Guid RecipientId { get; set; }
        [Required]
        public decimal PwAmount { get; set; }
    }
}
