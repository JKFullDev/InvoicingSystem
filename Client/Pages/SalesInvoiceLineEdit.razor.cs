using InvoicingSystem.Server.Data.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoicingSystem.Client.Pages
{
    public partial class SalesInvoiceLineEdit : ComponentBase
    {
        [Inject] protected NotificationService NotificationService { get; set; } = default!;
        [Inject] protected DialogService DialogService { get; set; } = default!;

        [Parameter] public bool IsNew { get; set; } = true;
        [Parameter] public SalesInvoiceLines? Line { get; set; }
        [Parameter] public IEnumerable<Products>? Products { get; set; }
        [Parameter] public IEnumerable<TaxRates>? TaxRates { get; set; }
        [Parameter] public EventCallback<SalesInvoiceLines?> OnClose { get; set; }  // Callback para cerrar sidebar

        private SalesInvoiceLines line = new()
        {
            SalesInvoiceHeaderId = "",
            CustomDescription = "",
            SalesInvoiceLineId = Guid.NewGuid(),
            ProductId = null,
            TaxRateId = null,
            UnitPrice = 0,
            Quantity = 1
        };

        protected override void OnInitialized()
        {
            if (!IsNew && Line != null)
            {
                // Hago una copia para editarla
                line = new SalesInvoiceLines
                {
                    SalesInvoiceLineId = Line.SalesInvoiceLineId,
                    SalesInvoiceHeaderId = Line.SalesInvoiceHeaderId,
                    ProductId = Line.ProductId,
                    TaxRateId = Line.TaxRateId,
                    UnitPrice = Line.UnitPrice,
                    Quantity = Line.Quantity,
                    CustomDescription = Line.CustomDescription
                };
            }
            else
            {
                // Inicializo una nueva línea
                line = new SalesInvoiceLines
                {
                    SalesInvoiceLineId = Guid.NewGuid(),
                    SalesInvoiceHeaderId = "",
                    ProductId = null,
                    TaxRateId = null,
                    UnitPrice = 0,
                    Quantity = 1,
                    CustomDescription = ""
                };
            }
        }

        private void OnProductChanged(object productId)
        {
            if (productId is Guid guidId && Products != null)
            {
                var selectedProduct = Products.FirstOrDefault(p => p.ProductId == guidId);
                if (selectedProduct != null)
                {
                    // Relleno el precio del producto seleccionado
                    line.UnitPrice = selectedProduct.CurrentPrice;
                }
            }
        }

        // Versión para @bind-Value:after
        private Task OnProductChangedAsync()
        {
            if (Products != null)
            {
                var selectedProduct = Products.FirstOrDefault(p => p.ProductId == line.ProductId);
                if (selectedProduct != null)
                {
                    // Relleno el precio del producto seleccionado
                    line.UnitPrice = selectedProduct.CurrentPrice;
                }
            }
            return Task.CompletedTask;
        }

        private async Task Save()
        {
            // Valido que haya producto
            if (line.ProductId == null || line.ProductId == Guid.Empty)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Validación",
                    Detail = "Debe seleccionar un producto",
                    Duration = 3000
                });
                return;
            }

            if (line.TaxRateId == null || line.TaxRateId == Guid.Empty)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Validación",
                    Detail = "Debe seleccionar un tipo de IVA",
                    Duration = 3000
                });
                return;
            }

            if (string.IsNullOrWhiteSpace(line.CustomDescription))
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Validación",
                    Detail = "Debe ingresar una descripción",
                    Duration = 3000
                });
                return;
            }

            // Cierro usando callback o diálogo
            if (OnClose.HasDelegate)
            {
                await OnClose.InvokeAsync(line);  // Sidebar
            }
            else
            {
                DialogService.Close(line);  // Fallback para modal
            }
        }

        private async Task Cancel()
        {
            // Cierro sin guardar
            if (OnClose.HasDelegate)
            {
                await OnClose.InvokeAsync(null);  // Sidebar
            }
            else
            {
                DialogService.Close(null);  // Fallback para modal
            }
        }
    }
}
