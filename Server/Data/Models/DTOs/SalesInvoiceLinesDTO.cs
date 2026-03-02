using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models.DTOs
{
    public class SalesInvoiceLinesDTO
    {
        [Key]
        public Guid SalesInvoiceLineId { get; set; }

        [Required]
        public required string SalesInvoiceHeaderId { get; set; }

        // Quito required para evitar problemas de serialización
        public SalesInvoiceHeadersDTO? SalesInvoiceHeader { get; set; }

        // Nullable para floating labels - Validación en cliente
        public Guid? ProductId { get; set; } // FK nullable

        // Quito required para evitar problemas de serialización
        public ProductsDTO? Product { get; set; }

        // Nullable para floating labels - Validación en cliente
        public Guid? TaxRateId { get; set; } // FK nullable

        // Quito required para evitar problemas de serialización
        public TaxRatesDTO? TaxRate { get; set; }

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
