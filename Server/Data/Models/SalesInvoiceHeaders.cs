using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models
{
    public class SalesInvoiceHeaders
    {
        [Key]
        public string SalesInvoiceHeaderId { get; set; }

        [Required]
        public string CustomerReference { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public string QuoteReference { get; set; }

        [Required]
        public string CustomerId { get; set; } //FK  
        public Customers Customer { get; set; } // La propiedad de navegación (para hacer .Include(x => x.Customer))

        [Required]
        public Guid PaymentTermsId { get; set; } //FK
        public PaymentTerms PaymentTerms { get; set; }

        [Required]
        // Relación 1-a-N con las líneas
        public List<SalesInvoiceLines> Lines { get; set; } = new();
    }
}
