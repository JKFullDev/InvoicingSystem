using System.ComponentModel.DataAnnotations;


namespace InvoicingSystem.Server.Data.Models.DTOs
{
    public class TaxRatesDTO
    {
        [Key]
        public Guid TaxRateId { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public decimal Percentage { get; set; }

    }
}
