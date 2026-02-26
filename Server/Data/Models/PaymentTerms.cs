using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models
{
    public class PaymentTerms
    {
        [Key]
        public Guid PaymentTermsId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int PaymentDays { get; set; }

    }
}
