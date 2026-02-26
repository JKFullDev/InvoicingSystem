using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models.DTOs
{
    public class SalesInvoiceHeadersDTO
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
        public CustomersDTO Customer { get; set; } // La propiedad de navegación (para hacer .Include(x => x.Customer))

        [Required]
        public Guid PaymentTermsId { get; set; } //FK
        public PaymentTermsDTO PaymentTerms { get; set; }

        public List<SalesInvoiceLinesDTO> Lines { get; set; } = new();  // Relación 1-a-N con las líneas
    }
}
