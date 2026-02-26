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
    public partial class CustomerList : ComponentBase
    {
        [Inject] protected ICustomersService CustomersService { get; set; } = default!;

        // Variables de la tabla
        protected RadzenDataGrid<Customers> grid = default!;
        protected IEnumerable<Customers>? customers;
        protected int count;
        protected bool isLoading = false;

        // Variables del formulario Maestro-Detalle
        protected bool showForm = false;
        protected bool isNew = false;
        protected Customers customerToEdit = new Customers();

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
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
            isNew = true;
            customerToEdit = new Customers(); // Vaciamos el formulario
            showForm = true;
        }

        protected void OnRowDoubleClick(DataGridRowMouseEventArgs<Customers> args)
        {
            // Barrera de seguridad (quita el CS8602)
            if (args.Data == null) return;

            isNew = false;

            // Clonamos asegurando que si algún campo es null, se ponga como cadena vacía
            customerToEdit = new Customers
            {
                CustomerId = args.Data.CustomerId ?? "",
                Name = args.Data.Name ?? "",
                Nif = args.Data.Nif ?? "",
                City = args.Data.City ?? "",
                Address = args.Data.Address ?? ""
            };

            showForm = true;
        }

        protected void CancelEdit()
        {
            showForm = false;
        }

        protected async Task SaveCustomer()
        {
            try
            {
                // Mapeamos a tu DTO para enviarlo al servidor
                var dto = new CustomersDTO
                {
                    CustomerId = customerToEdit.CustomerId,
                    Name = customerToEdit.Name,
                    Nif = customerToEdit.Nif,
                    City = customerToEdit.City,
                    Address = customerToEdit.Address
                };

                if (isNew)
                {
                    await CustomersService.CreateCustomers(dto);
                }
                else
                {
                    // Usamos tu método ReplaceCustomers (PUT)
                    await CustomersService.ReplaceCustomers(customerToEdit.CustomerId, dto);
                }

                showForm = false;
                await grid.Reload(); // Esto lanza LoadData automáticamente y refresca la vista
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar: {ex.Message}");
            }
        }

        protected async Task DeleteCustomer(string customerId)
        {
            try
            {
                var response = await CustomersService.DeleteCustomers(customerId);
                if (response.IsSuccessStatusCode)
                {
                    await grid.Reload();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al borrar: {ex.Message}");
            }
        }
    }
}