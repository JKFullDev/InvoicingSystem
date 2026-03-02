using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models
{
    public class SalesInvoiceLines
    {
        [Key]
        public Guid SalesInvoiceLineId { get; set; }

        [Required]
        public required string SalesInvoiceHeaderId { get; set; }

        // Quito required para evitar problemas de deserialización
        public SalesInvoiceHeaders? SalesInvoiceHeader { get; set; }

        // Nullable para floating labels - Validación en cliente con RadzenRequiredValidator
        public Guid? ProductId { get; set; } // FK Guid

        public Products? Product { get; set; }

        // Nullable para floating labels - Validación en cliente con RadzenRequiredValidator
        public Guid? TaxRateId { get; set; } // FK Guid

        public TaxRates? TaxRate { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public required string CustomDescription { get; set; }

        // Calculo el total de la línea
        public decimal TotalLine => UnitPrice * Quantity;
    }
}
