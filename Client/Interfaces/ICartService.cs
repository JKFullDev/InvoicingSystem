using InvoicingSystem.Server.Data.Models;

namespace InvoicingSystem.Client.Interfaces
{
    public interface ICartService
    {
        event Action? OnCartChanged;
        
        // Ver carrito
        Task<ShoppingCart> GetCartAsync();

        // Añadir al carrito
        Task AddToCartAsync(Products product, int quantity = 1);

        // Eliminar del carrito
        Task RemoveFromCartAsync(Guid productId);

        // Actualizar cantidad producto
        Task UpdateQuantityAsync(Guid productId, int quantity);

        // Vaciar carrito
        Task ClearCartAsync();

        // Nº de produtos totales
        int GetTotalItems();
    }
}