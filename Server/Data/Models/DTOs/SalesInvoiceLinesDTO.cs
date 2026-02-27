using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models.DTOs
{
    public class SalesInvoiceLinesDTO
    {
        [Key]
        public Guid SalesInvoiceLineId { get; set; }

        [Required]
        public required string SalesInvoiceHeaderId { get; set; }

        [Required]
        public required SalesInvoiceHeadersDTO SalesInvoiceHeader { get; set; }

        [Required]
        public Guid ProductId { get; set; } // FK Guid
        public required ProductsDTO Product { get; set; }

        [Required]
        public Guid TaxRateId { get; set; } // FK Guid
        public required TaxRatesDTO TaxRate { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public required string CustomDescription { get; set; }

        public decimal TotalLine => UnitPrice * Quantity;
    }
}
