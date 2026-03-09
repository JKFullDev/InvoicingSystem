using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace InvoicingSystem.Client.Components
{
    public partial class SelectCustomerDialog : ComponentBase
    {
        [Inject] protected ICustomersService CustomersService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;
        [Inject] protected DialogService DialogService { get; set; } = default!;

        private IEnumerable<Customers>? customers;
        private string? selectedCustomerId;

        protected override async Task OnInitializedAsync()
        {
            await LoadCustomers();
        }

        private async Task LoadCustomers()
        {
            try
            {
                var result = await CustomersService.GetCustomers(null);
                if (result != null)
                {
                    customers = result.Value;

                    if (customers != null && !customers.Any())
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Warning,
                            Summary = "Sin clientes",
                            Detail = "No hay clientes registrados. Por favor, cree un cliente primero.",
                            Duration = 4000
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error al cargar clientes",
                    Detail = ex.Message,
                    Duration = 4000
                });
            }
        }

        private void Submit()
        {
            if (!string.IsNullOrEmpty(selectedCustomerId))
            {
                DialogService.Close(selectedCustomerId);
            }
        }

        private void Cancel()
        {
            DialogService.Close(null);
        }
    }
}
