namespace InvoicingSystem.Server.Data.Models
{
    public class CartItem
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }

        public decimal Subtotal => Price * Quantity;
    }
}