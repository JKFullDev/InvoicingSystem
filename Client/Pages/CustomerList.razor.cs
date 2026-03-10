using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicingSystem.Client.Pages
{
    public partial class CustomerList : ComponentBase
    {
        [Inject] protected ICustomersService CustomersService { get; set; } = default!;
        [Inject] protected ISalesInvoiceHeadersService SalesInvoiceHeadersService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;
        [Inject] protected DialogService DialogService { get; set; } = default!;

        protected RadzenDataGrid<Customers> grid = default!;
        protected IEnumerable<Customers>? customers;
        protected int count;
        protected bool isLoading = true;
        private bool _firstRender = true;

        // Variables para el sidebar
        private bool sidebarExpanded = false;
        private string sidebarTitle = "";
        private bool isNewCustomer = true;
        private string? selectedCustomerId = null;

        protected override async Task OnInitializedAsync()
        {
            customers = new List<Customers>
            {
                new Customers { CustomerId = "", Name = "", Address = "", City = "", Nif = "" },
                new Customers { CustomerId = "", Name = "", Address = "", City = "", Nif = "" },
                new Customers { CustomerId = "", Name = "", Address = "", City = "", Nif = "" },
                new Customers { CustomerId = "", Name = "", Address = "", City = "", Nif = "" },
                new Customers { CustomerId = "", Name = "", Address = "", City = "", Nif = "" }
            };
            await InvokeAsync(StateHasChanged);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && _firstRender)
            {
                _firstRender = false;
                await Task.Delay(200);
                await grid.Reload();
            }
        }

        protected async Task LoadData(LoadDataArgs args)
        {
            isLoading = true;
            try
            {
                var query = new Query { Filter = args.Filter, OrderBy = args.OrderBy, Skip = args.Skip, Top = args.Top };
                var result = await CustomersService.GetCustomers(query);

                if (result != null)
                {
                    customers = result.Value;
                    count = result.Count;
                }
            }
            finally
            {
                isLoading = false;
            }
        }

        protected void GoToAdd()
        {
            sidebarTitle = "Nuevo Cliente";
            isNewCustomer = true;
            selectedCustomerId = null;
            sidebarExpanded = true;
        }

        protected void OnRowDoubleClick(DataGridRowMouseEventArgs<Customers> args)
        {
            if (args.Data == null) return;

            sidebarTitle = $"Editar Cliente: {args.Data.Name}";
            isNewCustomer = false;
            selectedCustomerId = args.Data.CustomerId;
            sidebarExpanded = true;
        }

        
        protected async Task OnRowExpand(Customers customer)
        {
            // 1. Comprobamos si las facturas ya están cargadas para no machacar la DB a peticiones
            if (customer.SalesInvoiceHeaders == null || !customer.SalesInvoiceHeaders.Any())
            {
                // 2. Llamamos al servicio OData filtrando por el ID de este cliente
                var result = await SalesInvoiceHeadersService.GetSalesInvoiceHeadersByCustomerId(customer.CustomerId);

                // 3. Si hay datos, los asignamos a la propiedad virtual del modelo
                if (result != null && result.Value != null)
                {
                    customer.SalesInvoiceHeaders = result.Value.ToList();
                }
            }
        }

        private async Task CloseSidebar(bool reload = false)
        {
            sidebarExpanded = false;

            if (reload)
            {
                await grid.Reload();
            }
        }

        protected async Task DeleteCustomer(string customerId)
        {
            try
            {
                var response = await CustomersService.DeleteCustomers(customerId);

                if (response.IsSuccessStatusCode)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Cliente eliminado correctamente",
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
                        Detail = "No se puede eliminar este cliente porque tiene facturas asociadas.",
                        Duration = 6000
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "No encontrado",
                        Detail = "El cliente no existe",
                        Duration = 4000
                    });
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Error al eliminar el cliente",
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
                    Detail = $"Error inesperado: {ex.Message}",
                    Duration = 4000
                });
            }
        }
    }
}
