using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models
{
    public class Products
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
