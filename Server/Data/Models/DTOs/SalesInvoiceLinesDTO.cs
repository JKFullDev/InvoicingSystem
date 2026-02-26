using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models.DTOs
{
    public class SalesInvoiceLinesDTO
    {
        [Key]
        public Guid SalesInvoiceLineId { get; set; }

        [Required]
        public string SalesInvoiceHeaderId { get; set; }

        [Required]
        public SalesInvoiceHeadersDTO SalesInvoiceHeader { get; set; }

        [Required]
        public Guid ProductId { get; set; } // FK Guid
        public ProductsDTO Product { get; set; }

        [Required]
        public Guid TaxRateId { get; set; } // FK Guid
        public TaxRatesDTO TaxRate { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string CustomDescription { get; set; }

        public decimal TotalLine => UnitPrice * Quantity;
    }
}
