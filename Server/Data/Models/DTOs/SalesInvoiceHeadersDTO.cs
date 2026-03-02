using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models.DTOs
{
    public class SalesInvoiceHeadersDTO
    {
        [Key]
        public required string SalesInvoiceHeaderId { get; set; }

        [Required]
        public required string CustomerReference { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public required string QuoteReference { get; set; }

        [Required]
        public required string CustomerId { get; set; } // FK

        // Quito required porque al enviar desde el cliente solo envío los IDs
        public CustomersDTO? Customer { get; set; }

        // Nullable para floating labels - Validación en cliente con RadzenRequiredValidator
        public Guid? PaymentTermsId { get; set; } // FK nullable

        // Quito required porque al enviar desde el cliente solo envío los IDs
        public PaymentTermsDTO? PaymentTerms { get; set; }

        public List<SalesInvoiceLinesDTO> Lines { get; set; } = new();
    }
}
