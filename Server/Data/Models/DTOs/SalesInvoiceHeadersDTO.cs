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
        public required string CustomerId { get; set; } //FK
        public required CustomersDTO Customer { get; set; } // La propiedad de navegación (para hacer .Include(x => x.Customer))

        [Required]
        public Guid PaymentTermsId { get; set; } //FK
        public required PaymentTermsDTO PaymentTerms { get; set; }

        public List<SalesInvoiceLinesDTO> Lines { get; set; } = new();  // Relación 1-a-N con las líneas
    }
}
