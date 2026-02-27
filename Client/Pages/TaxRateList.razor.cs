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
    public partial class TaxRateList : ComponentBase
    {
        [Inject] protected ITaxRatesService TaxRatesService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;

        // Variables de la tabla
        protected RadzenDataGrid<TaxRates> grid = default!;
        protected IEnumerable<TaxRates>? taxRates;
        protected int count;
        protected bool isLoading = false;

        // Variables del formulario Maestro-Detalle
        protected bool showForm = false;
        protected bool isNew = false;
        protected TaxRates taxRateToEdit = new TaxRates { Name = "" };

        // Indico si el taxRateo está en facturas
        protected bool isTaxRateInvoiced = false;
        protected int invoiceCount = 0;

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
                var result = await TaxRatesService.GetTaxRates(query);

                if (result != null)
                {
                    taxRates = result.Value;
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
            taxRateToEdit = new TaxRates { Name = ""};
            isTaxRateInvoiced = false; // Reseteo la advertencia
            invoiceCount = 0;
            showForm = true;
        }

        protected async Task OnRowDoubleClick(DataGridRowMouseEventArgs<TaxRates> args)
        {
            if (args.Data == null) return;

            isNew = false;

            // Clono el taxRateo para editarlo
            taxRateToEdit = new TaxRates
            {
                TaxRateId = args.Data.TaxRateId,
                Name = args.Data.Name ?? "",
                Percentage = args.Data.Percentage,
            };

            // Verifico si el taxRateo está en facturas para mostrar advertencia
            var (isInvoiced, count) = await TaxRatesService.IsTaxRateInvoiced(args.Data.TaxRateId);
            isTaxRateInvoiced = isInvoiced;
            invoiceCount = count;

            showForm = true;
        }

        protected void CancelEdit()
        {
            showForm = false;
        }

        protected async Task SaveTaxRate(TaxRates item)
        {
            try
            {
                // Mapeo el taxRateo a DTO para enviarlo al servidor
                var dto = new TaxRatesDTO
                {
                    TaxRateId = item.TaxRateId,
                    Name = item.Name,
                    Percentage = item.Percentage,
                };

                if (isNew)
                {
                    var createdTaxRate = await TaxRatesService.CreateTaxRates(dto);
                    if (createdTaxRate != null)
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Success,
                            Summary = "Éxito",
                            Detail = "TaxRateo creado correctamente",
                            Duration = 4000
                        });
                    }
                }
                else
                {
                    // Uso PUT para actualizar completamente el taxRateo
                    var response = await TaxRatesService.ReplaceTaxRates(item.TaxRateId, dto);

                    if (response.IsSuccessStatusCode)
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Success,
                            Summary = "Éxito",
                            Detail = "TaxRateo actualizado correctamente",
                            Duration = 4000
                        });
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Error,
                            Summary = "Error al actualizar",
                            Detail = $"No se pudo actualizar el taxRateo. Código: {response.StatusCode}",
                            Duration = 6000
                        });
                        Console.WriteLine($"Error: {errorContent}");
                        return; // No cierro el formulario si hay error
                    }
                }

                showForm = false;
                await grid.Reload(); // Recargo la tabla para mostrar los cambios
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Error al guardar: {ex.Message}",
                    Duration = 6000
                });
                Console.WriteLine($"Error al guardar: {ex.Message}");
            }
        }

        protected async Task DeleteTaxRate(Guid taxRateId)
        {
            try
            {
                var response = await TaxRatesService.DeleteTaxRates(taxRateId);

                if (response.IsSuccessStatusCode)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "TaxRateo eliminado correctamente",
                        Duration = 4000
                    });
                    await grid.Reload();
                    StateHasChanged();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // Error 409 Conflict - restricción de clave foránea
                    var errorContent = await response.Content.ReadAsStringAsync();
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "No se puede eliminar",
                        Detail = "No se puede eliminar este taxRateo porque está siendo utilizado en una o más facturas.",
                        Duration = 6000
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "No encontrado",
                        Detail = "El taxRateo no existe",
                        Duration = 4000
                    });
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Error al eliminar el taxRateo",
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

