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
    public partial class TaxRateEdit : ComponentBase
    {
        [Parameter] public Guid? TaxRateId { get; set; }
        [Parameter] public bool IsNew { get; set; } = false;
        [Parameter] public EventCallback<bool> OnClose { get; set; }  // Callback para cerrar sidebar

        private TaxRates? taxRate;
        private bool isLoading = true;

        protected override async Task OnInitializedAsync()
        {
            if (IsNew)
            {
                // Creo nuevo tax rate
                taxRate = new TaxRates
                {
                    TaxRateId = Guid.NewGuid(),
                    Name = "",
                    Percentage = 0
                };
            }
            else if (TaxRateId.HasValue)
            {
                // Cargo tax rate existente usando OData
                var query = new Query { Filter = $"TaxRateId eq {TaxRateId.Value}" };
                var result = await TaxRatesService.GetTaxRates(query);
                taxRate = result?.Value?.FirstOrDefault();
            }

            // Delay artificial para que se vea la animación de la barra de progreso
            await Task.Delay(500);

            isLoading = false;
        }

        private async Task OnSubmit()
        {
            if (taxRate == null) return;

            try
            {
                if (IsNew)
                {
                    var dto = new TaxRatesDTO
                    {
                        TaxRateId = taxRate.TaxRateId,
                        Name = taxRate.Name,
                        Percentage = taxRate.Percentage
                    };

                    await TaxRatesService.CreateTaxRates(dto);
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Tipo de IVA creado correctamente",
                        Duration = 4000
                    });
                }
                else
                {
                    await TaxRatesService.UpdateTaxRates(taxRate.TaxRateId, taxRate);
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Tipo de IVA actualizado correctamente",
                        Duration = 4000
                    });
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
