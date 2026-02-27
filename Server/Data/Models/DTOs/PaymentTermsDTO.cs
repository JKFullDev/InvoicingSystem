using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models.DTOs
{
    public class PaymentTermsDTO
    {
        [Key]
        public Guid PaymentTermsId { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public int PaymentDays { get; set; }

    }
}
