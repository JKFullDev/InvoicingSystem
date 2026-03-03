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
    public partial class CustomerEdit : ComponentBase
    {
        [Parameter] public string? CustomerId { get; set; }
        [Parameter] public bool IsNew { get; set; } = false;
        [Parameter] public EventCallback<bool> OnClose { get; set; }

        private Customers? customer;
        private bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            if (IsNew)
            {
                customer = new Customers
                {
                    CustomerId = "",
                    Name = "",
                    Address = "",
                    City = "",
                    Nif = ""
                };
            }
            else if (!string.IsNullOrEmpty(CustomerId))
            {
                var query = new Query { Filter = $"CustomerId eq '{CustomerId}'" };
                var result = await CustomersService.GetCustomers(query);
                customer = result?.Value?.FirstOrDefault();
            }

            await Task.Delay(500);
            isLoading = false;
        }

        private async Task OnSubmit()
        {
            if (customer == null) return;

            try
            {
                if (IsNew)
                {
                    var dto = new CustomersDTO
                    {
                        CustomerId = customer.CustomerId,
                        Name = customer.Name,
                        Address = customer.Address,
                        City = customer.City,
                        Nif = customer.Nif
                    };

                    await CustomersService.CreateCustomers(dto);
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Cliente creado correctamente",
                        Duration = 4000
                    });
                }
                else
                {
                    await CustomersService.UpdateCustomers(customer.CustomerId, customer);
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Cliente actualizado correctamente",
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
