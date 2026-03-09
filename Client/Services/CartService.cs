using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using Microsoft.JSInterop;
using System.Text.Json;

namespace InvoicingSystem.Client.Services
{
    public class CartService : ICartService
    {
        private const string STORAGE_KEY = "shopping_cart";  //Datos del usuario en navegador
        private readonly IJSRuntime _jsRuntime;  //Para poder tocar el localStorage (porque c# no puede)
        private ShoppingCart _cart = new();

        public event Action? OnCartChanged;

        public CartService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        // Obtener carrito
        public async Task<ShoppingCart> GetCartAsync()
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", STORAGE_KEY);
                
                //si hay objeto
                if (!string.IsNullOrEmpty(json))
                {
                    //se transforma ese texto a objeto c#
                    _cart = JsonSerializer.Deserialize<ShoppingCart>(json) ?? new ShoppingCart();
                }
            }
            catch
            {
                _cart = new ShoppingCart();
            }

            return _cart;
        }


        // Añadir al carrito
        public async Task AddToCartAsync(Products product, int quantity = 1)
        {
            await GetCartAsync();

            // Si el producto ya está en el carrito, se suma 1 en lugar de volver a añadirlo
            var existingItem = _cart.Items.FirstOrDefault(x => x.ProductId == product.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            // Si ese producto no existía en el carrito, se crea uno
            else
            {
                _cart.Items.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.CurrentPrice,
                    Quantity = quantity,
                    ImageUrl = product.ImageUrl
                });
            }

            // Se actualiza la feha y se guarda el carrito
            _cart.LastUpdated = DateTime.Now;
            await SaveCartAsync();
        }

        
        // Eliminar del carrito (Elimina todas las unidades de un producto)
        public async Task RemoveFromCartAsync(Guid productId)
        {
            await GetCartAsync();
            _cart.Items.RemoveAll(x => x.ProductId == productId);
            _cart.LastUpdated = DateTime.Now;
            await SaveCartAsync();
        }

        
        // Actualizar cantidades de un producto
        public async Task UpdateQuantityAsync(Guid productId, int quantity)
        {
            await GetCartAsync();
            var item = _cart.Items.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                // Si la cantidad pasa a ser 0 o menos, se elimina el producto
                if (quantity <= 0)
                {
                    await RemoveFromCartAsync(productId);
                }
                // Si la cantidad es mayor a 0 , se actualiza el número y se guarda
                else
                {
                    item.Quantity = quantity;
                    _cart.LastUpdated = DateTime.Now;
                    await SaveCartAsync();
                }
            }
        }


        //Vaciar todo el carrito
        public async Task ClearCartAsync()
        {
            _cart = new ShoppingCart();
            await SaveCartAsync();
        }


        // Obtener cantidad total de productos
        public int GetTotalItems()
        {
            return _cart.TotalItems;
        }


        // Guardar carrito (de uso interno)
        private async Task SaveCartAsync()
        {
            // Se convierte de c# a texto json
            var json = JsonSerializer.Serialize(_cart);
           
            // Se usa el puente de js para guardarlo en localstorage
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", STORAGE_KEY, json);
            OnCartChanged?.Invoke();
        }
    }
}