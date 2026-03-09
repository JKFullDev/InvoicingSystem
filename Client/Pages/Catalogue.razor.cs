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
            CartService.OnCartChanged += OnCartChangedHandler;
            await LoadData();
            await UpdateCartCount();
        }


        private async void OnCartChangedHandler()
        {
            // Obligamos a Blazor a procesar esto en el hilo de la interfaz de usuario (UI)
            await InvokeAsync(async () =>
            {
                await UpdateCartCount();
                StateHasChanged(); // Forzamos el repintado
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

        private async Task UpdateCartCount()
        {
            var cart = await CartService.GetCartAsync();
            cartItemCount = cart.TotalItems;
            StateHasChanged();
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

        // --- Añadir al carrito
        protected async Task AddToCart(Products product)
        {
            await CartService.AddToCartAsync(product);

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Producto añadido",
                Detail = $"{product.Name} añadido al carrito",
                Duration = 3000
            });
        }

        // --- Abrir carrito
        protected void ToggleCartSidebar()
        {
            isCartOpen = !isCartOpen;
            StateHasChanged();
        }


        // --- Cerrar carrito
        protected void CloseCartSidebar()
        {
            isCartOpen = false;
            StateHasChanged();
        }

        public void Dispose()
        {
            CartService.OnCartChanged -= OnCartChangedHandler;
        }
    }
}