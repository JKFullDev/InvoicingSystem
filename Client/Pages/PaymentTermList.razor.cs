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
    public partial class PaymentTermList : ComponentBase
    {
        [Inject] protected IPaymentTermsService PaymentTermsService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;
        [Inject] protected DialogService DialogService { get; set; } = default!;

        protected RadzenDataGrid<PaymentTerms> grid = default!;
        protected IEnumerable<PaymentTerms>? paymentTerms;
        protected int count;
        protected bool isLoading = true;
        private bool _firstRender = true;

        private bool sidebarExpanded = false;
        private string sidebarTitle = "";
        private bool isNewPaymentTerm = true;
        private Guid? selectedPaymentTermId = null;

        protected override async Task OnInitializedAsync()
        {
            paymentTerms = new List<PaymentTerms>
            {
                new PaymentTerms { PaymentTermsId = Guid.Empty, Description = "", PaymentDays = -1 },
                new PaymentTerms { PaymentTermsId = Guid.Empty, Description = "", PaymentDays = -1 },
                new PaymentTerms { PaymentTermsId = Guid.Empty, Description = "", PaymentDays = -1 },
                new PaymentTerms { PaymentTermsId = Guid.Empty, Description = "", PaymentDays = -1 },
                new PaymentTerms { PaymentTermsId = Guid.Empty, Description = "", PaymentDays = -1 }
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
            sidebarTitle = "Nuevo Término de Pago";
            isNewPaymentTerm = true;
            selectedPaymentTermId = null;
            sidebarExpanded = true;
        }

        protected void OnRowDoubleClick(DataGridRowMouseEventArgs<PaymentTerms> args)
        {
            if (args.Data == null) return;

            sidebarTitle = $"Editar: {args.Data.Description}";
            isNewPaymentTerm = false;
            selectedPaymentTermId = args.Data.PaymentTermsId;
            sidebarExpanded = true;
        }

        private async Task CloseSidebar(bool reload = false)
        {
            sidebarExpanded = false;

            if (reload)
            {
                await grid.Reload();
            }
        }

        protected async Task DeletePaymentTerm(Guid paymentTermId)
        {
            try
            {
                var response = await PaymentTermsService.DeletePaymentTerms(paymentTermId);

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
                        Detail = "No se puede eliminar este término porque está siendo utilizado en facturas.",
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
