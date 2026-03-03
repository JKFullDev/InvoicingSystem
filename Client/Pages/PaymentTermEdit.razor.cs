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
    public partial class PaymentTermEdit : ComponentBase
    {
        [Parameter] public Guid? PaymentTermsId { get; set; }
        [Parameter] public bool IsNew { get; set; } = false;
        [Parameter] public EventCallback<bool> OnClose { get; set; }

        private PaymentTerms? paymentTerm;
        private bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            if (IsNew)
            {
                paymentTerm = new PaymentTerms
                {
                    PaymentTermsId = Guid.NewGuid(),
                    Description = "",
                    PaymentDays = 30
                };
            }
            else if (PaymentTermsId.HasValue)
            {
                var query = new Query { Filter = $"PaymentTermsId eq {PaymentTermsId.Value}" };
                var result = await PaymentTermsService.GetPaymentTerms(query);
                paymentTerm = result?.Value?.FirstOrDefault();
            }

            await Task.Delay(500);
            isLoading = false;
        }

        private async Task OnSubmit()
        {
            if (paymentTerm == null) return;

            try
            {
                if (IsNew)
                {
                    var dto = new PaymentTermsDTO
                    {
                        PaymentTermsId = paymentTerm.PaymentTermsId,
                        Description = paymentTerm.Description,
                        PaymentDays = paymentTerm.PaymentDays
                    };

                    await PaymentTermsService.CreatePaymentTerms(dto);
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Término de pago creado correctamente",
                        Duration = 4000
                    });
                }
                else
                {
                    await PaymentTermsService.UpdatePaymentTerms(paymentTerm.PaymentTermsId, paymentTerm);
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Término de pago actualizado correctamente",
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
