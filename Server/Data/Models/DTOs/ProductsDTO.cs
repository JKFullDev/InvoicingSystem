using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models.DTOs
{
    public class ProductsDTO
    {
        [Key]
        public Guid ProductId { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Description { get; set; }

        [Required]
        public decimal CurrentPrice { get; set; }

        public string? ImageUrl { get; set; }

    }
}
