using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicingSystem.Client.Pages
{
    public partial class SalesInvoiceHeaderList : ComponentBase
    {
        //Servicio personalizado que consume la API REST (GET, POST, PUT, DELETE facturas)
        [Inject] protected ISalesInvoiceHeadersService SalesInvoiceHeadersService { get; set; } = default!;

        //Servicio de Radzen que muestra notificaciones de tipo toast (exito, error, advertencia)
        [Inject] protected NotificationService NotificationService { get; set; } = default!;

        //Servicio de Razden para abrir Modals. Lo uso para el formulario de edición
        [Inject] protected DialogService DialogService { get; set; } = default!;

        //Servicio de Radzem para gestionar navegación y URLs. Para construir la URL del PDF
        [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

        // Variables de la tabla
        protected RadzenDataGrid<SalesInvoiceHeaders> grid = default!;
        protected IEnumerable<SalesInvoiceHeaders>? salesInvoiceHeaders;
        protected int count;
        protected bool isLoading = true;
        private bool _firstRender = true;

        // Variables para el sidebar
        private bool sidebarExpanded = false;
        private string sidebarTitle = "";
        private bool isNewInvoice = true;
        private string? selectedInvoiceId = null;

        protected override async Task OnInitializedAsync()
        {
            // Inicializo con 5 objetos vacíos para mostrar el skeleton
            salesInvoiceHeaders = new List<SalesInvoiceHeaders> {
                new SalesInvoiceHeaders {
                    CustomerId = "",
                    CustomerReference = "",
                    QuoteReference = "",
                    SalesInvoiceHeaderId = ""
                },
                new SalesInvoiceHeaders {
                    CustomerId = "",
                    CustomerReference = "",
                    QuoteReference = "",
                    SalesInvoiceHeaderId = ""
                },
                new SalesInvoiceHeaders {
                    CustomerId = "",
                    CustomerReference = "",
                    QuoteReference = "",
                    SalesInvoiceHeaderId = ""
                },
                new SalesInvoiceHeaders {
                    CustomerId = "",
                    CustomerReference = "",
                    QuoteReference = "",
                    SalesInvoiceHeaderId = ""
                },
                new SalesInvoiceHeaders {
                    CustomerId = "",
                    CustomerReference = "",
                    QuoteReference = "",
                    SalesInvoiceHeaderId = ""
                }
            };
            await Task.CompletedTask;
        }

        // Después del primer renderizado, cargo los datos reales
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && _firstRender)
            {
                _firstRender = false;

                // Pequeño delay para que se vea el skeleton 
                await Task.Delay(200);

                // Cargo los datos reales reemplazando los placeholders
                await grid.Reload();
            }
        }


        // Carga de Datos con OData
        protected async Task LoadData(LoadDataArgs args)
        {
            isLoading = true;  // Activo spinner
            try
            {
                // Construyo la query OData con parámetros del DataGrid
                var query = new Query
                {
                    Filter = args.Filter,    // Ej: "CustomerReference eq 'ABC'"
                    OrderBy = args.OrderBy,  // Ej: "InvoiceDate desc"
                    Skip = args.Skip,        // Ej: 10 (saltar 10 registros)
                    Top = args.Top           // Ej: 10 (tomar 10 registros)
                };

                // Llamo al servicio que hace petición HTTP GET
                var result = await SalesInvoiceHeadersService.GetSalesInvoiceHeaders(query);

                if (result != null)
                {
                    salesInvoiceHeaders = result.Value;  // Las facturas reales reemplazan al skeleton
                    count = result.Count;                 // Total en BD
                }
            }
            finally
            {
                isLoading = false;  // Desactivo spinner siempre (incluso si hay error)
                await InvokeAsync(StateHasChanged);  // Fuerzo re-render para que se muestren los datos reales
            }
        }

        // Abro el sidebar para crear una nueva factura
        protected void GoToAdd()
        {
            sidebarTitle = "Nueva Factura";
            isNewInvoice = true;
            selectedInvoiceId = null;
            sidebarExpanded = true;
        }

        // Abro el sidebar para editar una factura existente
        protected void OnRowDoubleClick(DataGridRowMouseEventArgs<SalesInvoiceHeaders> args)
        {
            if (args.Data == null) return;

            sidebarTitle = $"Editar Factura: {args.Data.SalesInvoiceHeaderId}";
            isNewInvoice = false;
            selectedInvoiceId = args.Data.SalesInvoiceHeaderId;
            sidebarExpanded = true;
        }

        // Cierro el sidebar y recargo si es necesario
        private async Task CloseSidebar(bool reload = false)
        {
            sidebarExpanded = false;

            if (reload)
            {
                await grid.Reload();
            }
        }

        protected async Task DeleteSalesInvoiceHeader(string salesInvoiceHeaderId)
        {
            try
            {
                var response = await SalesInvoiceHeadersService.DeleteSalesInvoiceHeaders(salesInvoiceHeaderId);

                if (response.IsSuccessStatusCode)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Factura eliminada correctamente",
                        Duration = 4000
                    });
                    await grid.Reload();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "No se puede eliminar",
                        Detail = "No se puede eliminar esta factura porque tiene líneas asociadas.",
                        Duration = 6000
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "No encontrada",
                        Detail = "La factura no existe",
                        Duration = 4000
                    });
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Error al eliminar la factura",
                        Duration = 4000
                    });
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Excepción al eliminar: {ex.Message}",
                    Duration = 6000
                });
                Console.WriteLine($"Error al eliminar: {ex.Message}");
            }
        }

        // Descargo el PDF de una factura
        protected void DownloadPdf(string invoiceId)
        {
            try
            {
                // Construyo la URL de la API
                var pdfUrl = $"{NavigationManager.BaseUri}api/pdf/invoice/{invoiceId}";

                // Abro en nueva pestaña (el navegador descargará automáticamente)
                NavigationManager.NavigateTo(pdfUrl, forceLoad: true);

                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Generando PDF",
                    Detail = $"Generando PDF de la factura {invoiceId}...",
                    Duration = 3000
                });
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error al generar PDF",
                    Detail = ex.Message,
                    Duration = 6000
                });
            }
        }
    }
}