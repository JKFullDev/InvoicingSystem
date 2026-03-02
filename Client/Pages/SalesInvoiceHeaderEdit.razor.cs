using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoicingSystem.Client.Pages
{
    public partial class SalesInvoiceHeaderEdit : ComponentBase
    {
        [Parameter] public string? InvoiceId { get; set; }
        [Parameter] public bool IsNew { get; set; } = false;
        [Parameter] public EventCallback<bool> OnClose { get; set; }  // Callback para cerrar sidebar

        private SalesInvoiceHeaders? invoice;
        private List<SalesInvoiceLines> invoiceLines = new();
        private HashSet<Guid> linesToDelete = new(); // Líneas marcadas para eliminar
        private bool isLoading = true;

        // Listas para dropdowns
        private IEnumerable<Customers>? customers;
        private IEnumerable<PaymentTerms>? paymentTerms;
        private IEnumerable<Products>? products;
        private IEnumerable<TaxRates>? taxRates;
        private PaymentTerms? selectedPaymentTerm;

        protected override async Task OnInitializedAsync()
        {
            // Cargo datos maestros
            await LoadMasterData();

            if (IsNew)
            {
                // Creo nueva factura
                invoice = new SalesInvoiceHeaders
                {
                    SalesInvoiceHeaderId = "",
                    CustomerReference = "",
                    InvoiceDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30),
                    QuoteReference = "",
                    CustomerId = "",
                    PaymentTermsId = null  // null para que el floating label funcione
                };
                invoiceLines = new List<SalesInvoiceLines>();
            }
            else if (!string.IsNullOrEmpty(InvoiceId))
            {
                // Cargo factura existente con líneas usando OData $expand
                invoice = await SalesInvoiceHeadersService.GetSalesInvoiceHeaderById(InvoiceId);

                if (invoice != null)
                {
                    // Hago una copia profunda de las líneas para evitar problemas de referencia
                    invoiceLines = invoice.Lines?.Select(line => new SalesInvoiceLines
                    {
                        SalesInvoiceLineId = line.SalesInvoiceLineId,
                        SalesInvoiceHeaderId = line.SalesInvoiceHeaderId,
                        ProductId = line.ProductId,
                        TaxRateId = line.TaxRateId,
                        UnitPrice = line.UnitPrice,
                        Quantity = line.Quantity,
                        CustomDescription = line.CustomDescription
                    }).ToList() ?? new List<SalesInvoiceLines>();

                    selectedPaymentTerm = paymentTerms?.FirstOrDefault(pt => pt.PaymentTermsId == invoice.PaymentTermsId);

                    // Debug: Muestro cuántas líneas se cargaron
                    Console.WriteLine($"[DEBUG] Líneas cargadas: {invoiceLines.Count}");
                }
            }

            // Delay artificial para que se vea la animación de la barra de progreso
            await Task.Delay(500);  // 500ms para apreciar la animación

            isLoading = false;
        }

        private async Task LoadMasterData()
        {
            // Cargo clientes
            var customersResult = await CustomersService.GetCustomers(new Query { Top = 1000 });
            customers = customersResult?.Value;

            // Cargo condiciones de pago
            var paymentTermsResult = await PaymentTermsService.GetPaymentTerms(new Query { Top = 1000 });
            paymentTerms = paymentTermsResult?.Value;

            // Cargo productos
            var productsResult = await ProductsService.GetProducts(new Query { Top = 1000 });
            products = productsResult?.Value;

            // Cargo tipos de IVA
            var taxRatesResult = await TaxRatesService.GetTaxRates(new Query { Top = 1000 });
            taxRates = taxRatesResult?.Value;
        }

        private void OnInvoiceDateChanged(DateTime? newDate)
        {
            if (newDate.HasValue && invoice != null && selectedPaymentTerm != null)
            {
                invoice.DueDate = newDate.Value.AddDays(selectedPaymentTerm.PaymentDays);
            }
        }

        private void OnPaymentTermChanged(object paymentTermId)
        {
            if (paymentTermId is Guid guidId && invoice != null && paymentTerms != null)
            {
                selectedPaymentTerm = paymentTerms.FirstOrDefault(pt => pt.PaymentTermsId == guidId);

                if (selectedPaymentTerm != null)
                {
                    invoice.DueDate = invoice.InvoiceDate.AddDays(selectedPaymentTerm.PaymentDays);
                }
            }
        }

        // Versión async para usar con @bind-Value:after
        private Task OnPaymentTermChangedAsync()
        {
            if (invoice != null && paymentTerms != null)
            {
                selectedPaymentTerm = paymentTerms.FirstOrDefault(pt => pt.PaymentTermsId == invoice.PaymentTermsId);

                if (selectedPaymentTerm != null)
                {
                    invoice.DueDate = invoice.InvoiceDate.AddDays(selectedPaymentTerm.PaymentDays);
                }
            }
            return Task.CompletedTask;
        }

        private async Task AddLine()
        {
            // Abro diálogo para añadir línea
            var line = await DialogService.OpenAsync<SalesInvoiceLineEdit>("Nueva Línea",
                new Dictionary<string, object?>
                {
                    { "IsNew", true },
                    { "Products", products },
                    { "TaxRates", taxRates }
                },
                new DialogOptions { Width = "600px", Height = "auto" });

            if (line != null)
            {
                Console.WriteLine($"[DEBUG] AddLine - Línea recibida del diálogo: {line.SalesInvoiceLineId}");
                invoiceLines.Add(line);
                Console.WriteLine($"[DEBUG] AddLine - Total líneas ahora: {invoiceLines.Count}");

                // Fuerzo actualización del componente
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task EditLine(int index)
        {
            var originalLine = invoiceLines[index];

            // Abro diálogo para editar línea
            var line = await DialogService.OpenAsync<SalesInvoiceLineEdit>("Editar Línea",
                new Dictionary<string, object?>
                {
                    { "IsNew", false },
                    { "Line", originalLine },
                    { "Products", products },
                    { "TaxRates", taxRates }
                },
                new DialogOptions { Width = "600px", Height = "auto" });

            if (line != null)
            {
                invoiceLines[index] = line;
                StateHasChanged();
            }
        }

        // Método para manejar doble clic en una línea
        private async Task OnLineDoubleClick(DataGridRowMouseEventArgs<SalesInvoiceLines> args)
        {
            if (args.Data == null) return;

            // Busco el índice de la línea en la lista
            var index = invoiceLines.IndexOf(args.Data);

            if (index >= 0)
            {
                // Llamo al método EditLine existente
                await EditLine(index);
            }
        }

        private void DeleteLine(int index)
        {
            var lineId = invoiceLines[index].SalesInvoiceLineId;

            if (linesToDelete.Contains(lineId))
            {
                // Si ya estaba marcada, la desmarco (toggle)
                linesToDelete.Remove(lineId);
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Línea restaurada",
                    Detail = "La línea ya no se eliminará",
                    Duration = 3000
                });
            }
            else
            {
                // Marco para eliminar
                linesToDelete.Add(lineId);
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Warning,
                    Summary = "Línea marcada",
                    Detail = "La línea se eliminará al guardar",
                    Duration = 3000
                });
            }

            StateHasChanged();
        }

        // Método para aplicar estilos a las filas marcadas para eliminar
        private void RowRender(RowRenderEventArgs<SalesInvoiceLines> args)
        {
            if (linesToDelete.Contains(args.Data.SalesInvoiceLineId))
            {
                args.Attributes.Add("style", "background-color: #fee2e2; text-decoration: line-through; opacity: 0.7;");
            }
        }

        private decimal CalculateTotal()
        {
            // Sumo solo las líneas que NO están marcadas para eliminar
            return invoiceLines
                .Where(line => !linesToDelete.Contains(line.SalesInvoiceLineId))
                .Sum(line => line.TotalLine);
        }

        private async Task OnSubmit()
        {
            if (invoice == null) return;

            try
            {
                // Filtro las líneas que NO están marcadas para eliminar
                var linesToSave = invoiceLines
                    .Where(line => !linesToDelete.Contains(line.SalesInvoiceLineId))
                    .ToList();

                // Debug: Muestro cuántas líneas voy a enviar
                Console.WriteLine($"[DEBUG] OnSubmit - Líneas totales: {invoiceLines.Count}");
                Console.WriteLine($"[DEBUG] OnSubmit - Líneas marcadas para eliminar: {linesToDelete.Count}");
                Console.WriteLine($"[DEBUG] OnSubmit - Líneas a enviar: {linesToSave.Count}");

                foreach (var line in linesToSave)
                {
                    Console.WriteLine($"[DEBUG] Línea: {line.SalesInvoiceLineId} - Producto: {line.ProductId} - Cantidad: {line.Quantity}");
                }

                var dto = new SalesInvoiceHeadersDTO
                {
                    SalesInvoiceHeaderId = invoice.SalesInvoiceHeaderId,
                    CustomerReference = invoice.CustomerReference,
                    InvoiceDate = invoice.InvoiceDate,
                    DueDate = invoice.DueDate,
                    QuoteReference = invoice.QuoteReference,
                    CustomerId = invoice.CustomerId,
                    PaymentTermsId = invoice.PaymentTermsId,
                    Lines = linesToSave.Select(line => new SalesInvoiceLinesDTO
                    {
                        SalesInvoiceLineId = line.SalesInvoiceLineId,
                        SalesInvoiceHeaderId = invoice.SalesInvoiceHeaderId,
                        ProductId = line.ProductId,
                        TaxRateId = line.TaxRateId,
                        UnitPrice = line.UnitPrice,
                        Quantity = line.Quantity,
                        CustomDescription = line.CustomDescription
                    }).ToList()
                };

                Console.WriteLine($"[DEBUG] DTO Lines: {dto.Lines?.Count ?? 0}");

                if (IsNew)
                {
                    await SalesInvoiceHeadersService.CreateSalesInvoiceHeaders(dto);
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Factura creada correctamente",
                        Duration = 4000
                    });
                }
                else
                {
                    var response = await SalesInvoiceHeadersService.ReplaceSalesInvoiceHeaders(invoice.SalesInvoiceHeaderId, dto);

                    if (response.IsSuccessStatusCode)
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Success,
                            Summary = "Éxito",
                            Detail = "Factura actualizada correctamente",
                            Duration = 4000
                        });
                    }
                }

                // Cierro usando el callback (sidebar o dialog)
                if (OnClose.HasDelegate)
                {
                    await OnClose.InvokeAsync(true);  // true = guardó correctamente
                }
                else
                {
                    DialogService.Close(true);  // Fallback para cuando se usa como dialog
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
            // Cierro sin guardar
            if (OnClose.HasDelegate)
            {
                await OnClose.InvokeAsync(false);  // false = no guardó
            }
            else
            {
                DialogService.Close(false);  // Fallback para cuando se usa como dialog
            }
        }
    }
}
