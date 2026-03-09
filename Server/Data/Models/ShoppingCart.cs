namespace InvoicingSystem.Server.Data.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new();
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public int TotalItems => Items.Sum(x => x.Quantity);
        public decimal Subtotal => Items.Sum(x => x.Subtotal);
        public decimal Tax => Subtotal * 0.21m; // IVA 21%
        public decimal Total => Subtotal + Tax;
    }
}