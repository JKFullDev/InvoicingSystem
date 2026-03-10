using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InvoicingSystem.Server.Data.Models
{
    public class Customers
    {
        [Key]
        public required string CustomerId { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Address { get; set; }

        [Required]
        public required string City { get; set; }

        [Required]
        public required string Nif { get; set; }


        // Propiedad de navegación inversa - NO se serializa para evitar ciclos
        [JsonIgnore]
        public virtual ICollection<SalesInvoiceHeaders> SalesInvoiceHeaders { get; set; } = new List<SalesInvoiceHeaders>();

    }
}
