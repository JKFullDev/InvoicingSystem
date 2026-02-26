using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models
{
    public class SalesInvoiceLines
    {
        [Key]
        public Guid SalesInvoiceLineId { get; set; }

        [Required]
        public string SalesInvoiceHeaderId { get; set; }

        [Required]
        public SalesInvoiceHeaders SalesInvoiceHeader { get; set; }

        [Required]
        public Guid ProductId { get; set; } // FK Guid

        [Required]
        public Products Product { get; set; }

        [Required]
        public Guid TaxRateId { get; set; } // FK Guid

        [Required]
        public TaxRates TaxRate { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string CustomDescription { get; set; }

        public decimal TotalLine => UnitPrice * Quantity;

    }
}
