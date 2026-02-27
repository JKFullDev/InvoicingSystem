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
        public required string CustomerId { get; set; } //FK  
        public required Customers Customer { get; set; } // La propiedad de navegación (para hacer .Include(x => x.Customer))

        [Required]
        public Guid PaymentTermsId { get; set; } //FK
        public required PaymentTerms PaymentTerms { get; set; }

        [Required]
        // Relación 1-a-N con las líneas
        public List<SalesInvoiceLines> Lines { get; set; } = new();
    }
}
