using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace InvoicingSystem.Client.Components
{
    // Componente que gestiona el sidebar lateral del carrito de compras
    public partial class CartSidebar : ComponentBase
    {
        // Inyecto todos los servicios que necesito para manejar el carrito y crear facturas
        [Inject] protected ICartService CartService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;
        [Inject] protected ISalesInvoiceHeadersService SalesInvoiceHeadersService { get; set; } = default!;
        [Inject] protected ISalesInvoiceLinesService SalesInvoiceLinesService { get; set; } = default!;
        [Inject] protected ICustomersService CustomersService { get; set; } = default!;
        [Inject] protected IPaymentTermsService PaymentTermsService { get; set; } = default!;
        [Inject] protected ITaxRatesService TaxRatesService { get; set; } = default!;
        [Inject] protected DialogService DialogService { get; set; } = default!;

        // Parámetros que recibo desde el componente padre (Catalogue)
        [Parameter] public bool IsOpen { get; set; }
        [Parameter] public EventCallback OnClose { get; set; }

        // Variable privada donde guardo el carrito actual
        private ShoppingCart? cart;

        // Cuando cambian los parámetros, cargo el carrito si el sidebar está abierto
        protected override async Task OnParametersSetAsync()
        {
            if (IsOpen)
            {
                await LoadCart();
            }
        }

        // Cargo el carrito desde el servicio y actualizo la vista
        private async Task LoadCart()
        {
            cart = await CartService.GetCartAsync();
            StateHasChanged();
        }

        // Cierro el sidebar
        private async Task Close()
        {
            await OnClose.InvokeAsync();
        }

        // Incremento en 1 la cantidad de un producto
        private async Task IncreaseQuantity(CartItem item)
        {
            await CartService.UpdateQuantityAsync(item.ProductId, item.Quantity + 1);
            await LoadCart();
        }

        // Decremento en 1 la cantidad (si llega a 0 se elimina automáticamente)
        private async Task DecreaseQuantity(CartItem item)
        {
            await CartService.UpdateQuantityAsync(item.ProductId, item.Quantity - 1);
            await LoadCart();
        }

        // Elimino completamente un producto del carrito y muestro notificación
        private async Task RemoveItem(CartItem item)
        {
            await CartService.RemoveFromCartAsync(item.ProductId);
            await LoadCart();

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Producto eliminado",
                Detail = $"{item.Name} eliminado del carrito",
                Duration = 2000
            });
        }


        // Vacío todo el carrito de golpe
        private async Task ClearCart()
        {
            await CartService.ClearCartAsync();
            await LoadCart();

            NotificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Info,
                Summary = "Carrito vacío",
                Detail = "Se han eliminado todos los productos",
                Duration = 2000
            });
        }

        // Método principal que maneja todo el proceso de checkout
        // Primero valido, luego pido confirmación, creo la factura y descargo el PDF
        private async Task Checkout()
        {
            // Verifico que hay productos en el carrito
            if (cart == null || !cart.Items.Any())
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Carrito vacío",
                    Detail = "No hay productos en el carrito para facturar",
                    Duration = 3000
                });
                return;
            }

            try
            {
                // PASO 1: Muestro un dialog para que seleccione el cliente
                // PASO 1: Muestro un dialog para que seleccione el cliente
                // Le doy el mismo tamaño que el carrito para que sea consistente visualmente
                var result = await DialogService.OpenAsync<SelectCustomerDialog>("Seleccionar Cliente", 
                    null,
                    new DialogOptions 
                    { 
                        Width = "600px",
                        Top="50px",
                        Left="-5px",
                        Height = "100vh",
                        Resizable = false,
                        Draggable = false,
                        ShowClose = true,                    
                    });

                // El diálogo devuelve el CustomerId o null si cancela
                var customerId = result as string;

                if (string.IsNullOrWhiteSpace(customerId))
                {
                    return; // Si cancela, salgo sin hacer nada
                }

                // PASO 2: Pido confirmación mostrando el resumen del pedido
                var confirmed = await DialogService.Confirm(
                    $"¿Desea crear una factura con {cart.TotalItems} productos por un total de {cart.Total:C2}?",
                    "Confirmar Factura",
                    new ConfirmOptions { OkButtonText = "Sí, crear", CancelButtonText = "Cancelar" });

                if (confirmed != true)
                {
                    return; // Si no confirma, salgo
                }

                // Muestro notificación de que estoy procesando
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Creando factura",
                    Detail = "Generando factura de venta...",
                    Duration = 2000
                });

                // PASO 3: Creo la cabecera de la factura
                // Primero obtengo un PaymentTerms por defecto 
                Guid? defaultPaymentTermsId = await GetDefaultPaymentTermsId();

                var invoiceHeader = new InvoicingSystem.Server.Data.Models.DTOs.SalesInvoiceHeadersDTO
                {
                    SalesInvoiceHeaderId = Guid.NewGuid().ToString(),
                    CustomerId = customerId,
                    CustomerReference = $"WEB-{DateTime.Now:yyyyMMddHHmmss}", // Referencia única con timestamp
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30), 
                    QuoteReference = $"WEB-CART-{DateTime.Now:yyyyMMddHHmmss}",
                    PaymentTermsId = defaultPaymentTermsId ?? throw new Exception("No hay términos de pago disponibles. Por favor, cree al menos uno.")
                };

                SalesInvoiceHeaders? createdHeader = null;

                try
                {
                    createdHeader = await SalesInvoiceHeadersService.CreateSalesInvoiceHeaders(invoiceHeader);
                }
                catch (Exception createEx)
                {
                    throw new Exception($"Error al crear la cabecera: {createEx.Message}", createEx);
                }

                if (createdHeader == null)
                {
                    throw new Exception("La cabecera de la factura no se creó correctamente (null)");
                }

                // PASO 4: Creo las líneas de factura a partir de los productos del carrito
                // Primero obtengo un TaxRate por defecto (también necesito al menos uno en la BD)
                Guid? defaultTaxRateId = await GetDefaultTaxRateId();

                if (defaultTaxRateId == null)
                {
                    throw new Exception("No hay tasas de impuesto disponibles. Por favor, cree al menos una.");
                }

                int lineNumber = 0;
                foreach (var item in cart.Items)
                {
                    lineNumber++;
                    try
                    {
                        var invoiceLine = new InvoicingSystem.Server.Data.Models.DTOs.SalesInvoiceLinesDTO
                        {
                            SalesInvoiceLineId = Guid.NewGuid(), // Genero un ID único para cada línea
                            SalesInvoiceHeaderId = createdHeader.SalesInvoiceHeaderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            UnitPrice = item.Price,
                            CustomDescription = item.Description,
                            TaxRateId = defaultTaxRateId.Value
                        };

                        await SalesInvoiceLinesService.CreateSalesInvoiceLines(invoiceLine);
                    }
                    catch (Exception lineEx)
                    {
                        // Si falla una línea, muestro cuál fue para facilitar el debug
                        throw new Exception($"Error al crear la línea {lineNumber} (Producto: {item.Name}): {lineEx.Message}", lineEx);
                    }
                }

                // PASO 5: Vacío el carrito ya que la compra está completa
                await CartService.ClearCartAsync();
                await LoadCart();

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Factura creada",
                    Detail = $"Factura {createdHeader.SalesInvoiceHeaderId} creada correctamente",
                    Duration = 3000
                });

                // PASO 6: Redirijo al navegador para descargar el PDF de la factura
                var pdfUrl = $"{NavigationManager.BaseUri}api/pdf/invoice/{createdHeader.SalesInvoiceHeaderId}";
                NavigationManager.NavigateTo(pdfUrl, forceLoad: true);

                // PASO 7: Cierro el sidebar
                await Close();
            }
            catch (Exception ex)
            {
                // Si algo falla en cualquier punto, muestro el error al usuario
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error al crear factura",
                    Detail = ex.Message,
                    Duration = 6000
                });
            }
        }

        // Obtengo el primer PaymentTerms disponible en la BD
        private async Task<Guid?> GetDefaultPaymentTermsId()
        {
            try
            {
                var result = await PaymentTermsService.GetPaymentTerms(new Query());
                if (result?.Value != null && result.Value.Any())
                {
                    return result.Value.First().PaymentTermsId;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        // Devuelvo el TaxRate del IVA 21% (8D770245-9AF0-4585-908D-E3178E32E8AB)
        private Task<Guid?> GetDefaultTaxRateId()
        {
            // Uso directamente el GUID del IVA 21%
            return Task.FromResult<Guid?>(new Guid("8D770245-9AF0-4585-908D-E3178E32E8AB"));
        }

        // Método auxiliar para obtener la imagen de un producto
        // Manejo diferentes tipos de URLs: http/https, base64 o rutas locales
        private string GetImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return "images/no-image.png";

            var url = imageUrl.Trim();

            // Si ya es una URL completa o una imagen base64, la devuelvo tal cual
            if (url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("data:image/"))
                return url;

            // Si no, asumo que es una ruta local y le añado la barra inicial si hace falta
            return url.StartsWith("/") ? url : $"/{url}";
        }
    }
}
