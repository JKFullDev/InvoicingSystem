using System.ComponentModel.DataAnnotations;

namespace InvoicingSystem.Server.Data.Models.DTOs
{
    public class ProductsDTO
    {
        [Key]
        public Guid ProductId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal CurrentPrice { get; set; }

    }
}
