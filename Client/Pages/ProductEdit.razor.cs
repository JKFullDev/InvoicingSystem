using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InvoicingSystem.Client.Pages
{
    public partial class ProductEdit : ComponentBase
    {
        [Parameter] public Guid? ProductId { get; set; }
        [Parameter] public bool IsNew { get; set; } = false;
        [Parameter] public EventCallback<bool> OnClose { get; set; }

        private Products? product;
        private bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            if (IsNew)
            {
                product = new Products
                {
                    ProductId = Guid.NewGuid(),
                    Name = "",
                    Description = "",
                    CurrentPrice = 0
                };
            }
            else if (ProductId.HasValue)
            {
                var query = new Query { Filter = $"ProductId eq {ProductId.Value}" };
                var result = await ProductsService.GetProducts(query);
                product = result?.Value?.FirstOrDefault();
            }

            await Task.Delay(500);
            isLoading = false;
        }

        private async Task OnSubmit()
        {
            if (product == null) return;

            try
            {
                if (IsNew)
                {
                    var dto = new ProductsDTO
                    {
                        ProductId = product.ProductId,
                        Name = product.Name,
                        Description = product.Description,
                        CurrentPrice = product.CurrentPrice
                    };

                    await ProductsService.CreateProducts(dto);
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Producto creado correctamente",
                        Duration = 4000
                    });
                }
                else
                {
                    await ProductsService.UpdateProducts(product.ProductId, product);
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Producto actualizado correctamente",
                        Duration = 4000
                    });
                }

                if (OnClose.HasDelegate)
                {
                    await OnClose.InvokeAsync(true);
                }
                else
                {
                    DialogService.Close(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] OnSubmit: {ex.Message}");
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Error al guardar: {ex.Message}",
                    Duration = 6000
                });
            }
        }

        private async Task Cancel()
        {
            if (OnClose.HasDelegate)
            {
                await OnClose.InvokeAsync(false);
            }
            else
            {
                DialogService.Close(false);
            }
        }
    }
}
