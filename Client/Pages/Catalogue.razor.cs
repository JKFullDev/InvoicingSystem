using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace InvoicingSystem.Client.Pages
{
    public partial class Catalogue : ComponentBase, IDisposable
    {
        [Inject] protected IProductsService IProductsService { get; set; } = default!;
        [Inject] protected ICartService CartService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;

        protected IEnumerable<Products>? products;
        protected int count;
        protected bool isLoading = true;
        protected bool isCartOpen = false;
        protected int cartItemCount = 0;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            // Me suscribo al evento del carrito para actualizar el badge
            CartService.OnCartChanged += OnCartChangedHandler;

            // Cargo datos y el contador del carrito
            await LoadData();
            await UpdateCartCount();
        }


        // Este método se ejecuta cada vez que cambia el carrito
        private async void OnCartChangedHandler()
        {
            Console.WriteLine("[BADGE] OnCartChangedHandler disparado");

            // Ejecuto en el hilo de la UI para evitar problemas de concurrencia
            await InvokeAsync(async () =>
            {
                await UpdateCartCount();
                StateHasChanged(); // Fuerzo el repintado del componente
            });
        }

        protected async Task LoadData()
        {
            try
            {
                isLoading = true;
                var result = await IProductsService.GetProducts(null);

                if (result != null)
                {
                    products = result.Value;
                    count = result.Count;
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"No se pudieron cargar los productos: {ex.Message}",
                    Duration = 4000
                });
            }
            finally
            {
                isLoading = false;
            }
        }

        // Actualizo el contador del carrito
        private async Task UpdateCartCount()
        {
            try
            {
                var cart = await CartService.GetCartAsync();
                var previousCount = cartItemCount;
                cartItemCount = cart?.TotalItems ?? 0;

                // Log para debugging
                Console.WriteLine($"[BADGE] UpdateCartCount: {previousCount} → {cartItemCount}");

                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error al actualizar contador del carrito: {ex.Message}");
                cartItemCount = 0;
            }
        }

        // Obtengo el texto del badge (solo si hay items)
        private string? GetBadgeText()
        {
            return cartItemCount > 0 ? cartItemCount.ToString() : null;
        }

        // Metodo para obtener la imagen del producto (base64, url de internet o ruta local)
        private string GetProductImage(Products product)
        {
            if (string.IsNullOrWhiteSpace(product.ImageUrl))
                return "images/no-image.png";

            var imageUrl = product.ImageUrl.Trim();

            // Url internet
            if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || // que empiece exactamente por el texto entre comillas"
                imageUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return imageUrl;

            // Base 64
            if (imageUrl.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
                return imageUrl;

            // Ruta local
            return imageUrl.StartsWith("/") ? imageUrl : $"/{imageUrl}";
        }

        // Añado producto al carrito
        protected async Task AddToCart(Products product)
        {
            Console.WriteLine($"[BADGE] Añadiendo producto al carrito: {product.Name}");

            await CartService.AddToCartAsync(product);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Producto añadido",
                Detail = $"{product.Name} añadido al carrito",
                Duration = 3000
            });
        }

        // Abro/cierro el carrito
        protected async Task ToggleCartSidebar()
        {
            isCartOpen = !isCartOpen;

            // Si lo estoy abriendo, actualizo el contador por si acaso
            if (isCartOpen)
            {
                await UpdateCartCount();
            }

            StateHasChanged();
        }


        // Cierro el carrito
        protected async Task CloseCartSidebar()
        {
            isCartOpen = false;

            // Cuando cierro, actualizo el contador (por si se vació desde el checkout)
            await UpdateCartCount();

            StateHasChanged();
        }

        public void Dispose()
        {
            CartService.OnCartChanged -= OnCartChangedHandler;
        }
    }
}