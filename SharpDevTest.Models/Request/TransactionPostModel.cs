using System;
using System.ComponentModel.DataAnnotations;

namespace SharpDevTest.Models.Request
{
    public class TransactionPostModel
    {
        
        [Required]
        public string RecipientName { get; set; }
        [Required]
        [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "Value of {0} should be between {1:c} and {2:c}")]
        public decimal PwAmount { get; set; }
    }
}
