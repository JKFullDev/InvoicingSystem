using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicingSystem.Client.Pages
{
    public partial class CustomerList : ComponentBase
    {
        // Inyección de dependencias como propiedades
        [Inject]
        protected ICustomersService CustomersService { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        // Variables de estado protegidas para que la vista (.razor) pueda acceder a ellas
        protected IEnumerable<Customers>? customers;
        protected int count;
        protected bool isLoading = false;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
        }

        protected async Task LoadData(LoadDataArgs args)
        {
            isLoading = true;
            try
            {
                var query = new Query()
                {
                    Filter = args.Filter,
                    OrderBy = args.OrderBy,
                    Skip = args.Skip,
                    Top = args.Top
                };

                var result = await CustomersService.GetCustomers(query);

                if (result != null)
                {
                    customers = result.Value;

                    count = result.Count;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando datos: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        protected void GoToAdd()
        {
            NavigationManager.NavigateTo("/customers/add");
        }

        protected void OnRowDoubleClick(DataGridRowMouseEventArgs<Customers> args)
        {
            if (args.Data != null && args.Data.CustomerId != null)
            {
                GoToEdit(args.Data.CustomerId);
            }
        }

        protected void GoToEdit(string customerId)
        {
            NavigationManager.NavigateTo($"/customers/edit/{customerId}");
        }

        protected async Task DeleteCustomer(string customerId)
        {
            try
            {
                var response = await CustomersService.DeleteCustomers(customerId);

                if (response.IsSuccessStatusCode)
                {
                    var query = new Query { Top = 10, Skip = 0 };
                    var result = await CustomersService.GetCustomers(query);

                    if (result != null)
                    {
                        customers = result.Value;

                        count = result.Count;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al borrar: {ex.Message}");
            }
        }
    }
}