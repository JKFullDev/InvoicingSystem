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
    public partial class PaymentTermsList : ComponentBase
    {
        [Inject] protected IPaymentTermsService PaymentTermsService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;

        // Variables de la tabla
        protected RadzenDataGrid<PaymentTerms> grid = default!;
        protected IEnumerable<PaymentTerms>? paymentTerms;
        protected int count;
        protected bool isLoading = false;

        // Variables del formulario Maestro-Detalle
        protected bool showForm = false;
        protected bool isNew = false;
        protected PaymentTerms paymentTermsToEdit = new PaymentTerms { Description = "" };

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
                var result = await PaymentTermsService.GetPaymentTerms(query);

                if (result != null)
                {
                    paymentTerms = result.Value;
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
            paymentTermsToEdit = new PaymentTerms { Description = "" }; // Vaciamos el formulario
            showForm = true;
        }

        protected void OnRowDoubleClick(DataGridRowMouseEventArgs<PaymentTerms> args)
        {
            if (args.Data == null) return;

            isNew = false;

            // Clonamos asegurando que si algún campo es null, se ponga como cadena vacía
            paymentTermsToEdit = new PaymentTerms
            {
                PaymentTermsId = args.Data.PaymentTermsId,
                Description = args.Data.Description ?? "",
                PaymentDays = args.Data.PaymentDays
            };

            showForm = true;
        }

        protected void CancelEdit()
        {
            showForm = false;
        }

        protected async Task SavePaymentTerms()
        {
            try
            {
                // Mapeamos a tu DTO para enviarlo al servidor
                var dto = new PaymentTermsDTO
                {
                    PaymentTermsId = paymentTermsToEdit.PaymentTermsId,
                    Description = paymentTermsToEdit.Description,
                    PaymentDays = paymentTermsToEdit.PaymentDays,
                };

                if (isNew)
                {
                    await PaymentTermsService.CreatePaymentTerms(dto);
                }
                else
                {
                    // Usamos tu método ReplacePaymentTerms (PUT)
                    await PaymentTermsService.ReplacePaymentTerms(paymentTermsToEdit.PaymentTermsId, dto);
                }

                showForm = false;
                await grid.Reload(); // Esto lanza LoadData automáticamente y refresca la vista
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar: {ex.Message}");
            }
        }

        protected async Task DeletePaymentTerms(Guid paymentTermsId)
        {
            try
            {
                var response = await PaymentTermsService.DeletePaymentTerms(paymentTermsId);

                if (response.IsSuccessStatusCode)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Término de pago eliminado correctamente",
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
                        Detail = "No se puede eliminar este término de pago porque está siendo utilizado en una o más facturas.",
                        Duration = 6000
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "No encontrado",
                        Detail = "El término de pago no existe",
                        Duration = 4000
                    });
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Error al eliminar el término de pago",
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