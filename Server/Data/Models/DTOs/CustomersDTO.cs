using System.ComponentModel.DataAnnotations;
namespace InvoicingSystem.Server.Data.Models.DTOs
{
    public class CustomersDTO
    {
        [Key]
        public string CustomerId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Nif { get; set; }

    }
}
