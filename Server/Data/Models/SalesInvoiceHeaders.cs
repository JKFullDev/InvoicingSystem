using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models
{
    public class SalesInvoiceHeaders
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

        // Quito required porque al deserializar desde JSON puede ser null
        public Customers? Customer { get; set; }

        // Nullable para floating labels - Validación en cliente con RadzenRequiredValidator
        public Guid? PaymentTermsId { get; set; } // FK nullable

        // Quito required porque al deserializar desde JSON puede ser null
        public PaymentTerms? PaymentTerms { get; set; }

        // Relación 1-a-N con las líneas
        public List<SalesInvoiceLines> Lines { get; set; } = new();
    }
}
