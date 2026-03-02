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
                // Clono la línea para editarla
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
                // Nueva línea
                line = new SalesInvoiceLines
                {
                    SalesInvoiceLineId = Guid.NewGuid(),
                    SalesInvoiceHeaderId = "",
                    ProductId = null,  // null para floating label
                    TaxRateId = null,  // null para floating label
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
                    // Copio el precio actual del producto
                    line.UnitPrice = selectedProduct.CurrentPrice;
                }
            }
        }

        // Versión async para usar con @bind-Value:after
        private Task OnProductChangedAsync()
        {
            if (Products != null)
            {
                var selectedProduct = Products.FirstOrDefault(p => p.ProductId == line.ProductId);
                if (selectedProduct != null)
                {
                    // Copio el precio actual del producto
                    line.UnitPrice = selectedProduct.CurrentPrice;
                }
            }
            return Task.CompletedTask;
        }

        private void Save()
        {
            // Valido
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

            // Cierro el diálogo y devuelvo la línea
            DialogService.Close(line);
        }

        private void Cancel()
        {
            // Cierro el diálogo sin guardar
            DialogService.Close(null);
        }
    }
}
