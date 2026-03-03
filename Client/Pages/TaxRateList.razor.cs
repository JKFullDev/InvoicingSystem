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

        //Servicio de Razden para abrir Modals. Lo uso para el formulario de edición
        [Inject] protected DialogService DialogService { get; set; } = default!;

        // Variables de la tabla
        protected RadzenDataGrid<TaxRates> grid = default!;
        protected IEnumerable<TaxRates>? taxRates;
        protected int count;
        protected bool isLoading = true;
        private bool _firstRender = true;

        // Variables para el sidebar
        private bool sidebarExpanded = false;
        private string sidebarTitle = "";
        private bool isNewTaxRate = true;
        private Guid? selectedTaxRateId = null;  // Cambio a Guid? porque TaxRateId es Guid

        // Indico si el taxRateo está en facturas
        protected bool isTaxRateInvoiced = false;
        protected int invoiceCount = 0;

        protected override async Task OnInitializedAsync()
        {
            taxRates = new List<TaxRates>
            {
                new TaxRates
                {
                    Name="",
                    Percentage=-1
                },
                new TaxRates
                {
                    Name="",
                    Percentage=-1
                },
                new TaxRates
                {
                    Name="",
                    Percentage=-1
                },
                new TaxRates
                {
                    Name="",
                    Percentage=-1
                },
                new TaxRates
                {
                    Name="",
                    Percentage=-1
                }
            };
            await InvokeAsync(StateHasChanged);
        }

        // Después del primer renderizado, cargo los datos reales
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && _firstRender)
            {
                _firstRender = false;

                // Pequeño delay para que se vea el skeleton 
                await Task.Delay(200);

                // Cargo los datos reales reemplazando los placeholders
                await grid.Reload();
            }
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
            sidebarTitle = "Nuevo Impuesto";
            isNewTaxRate = true;
            selectedTaxRateId = null;
            sidebarExpanded = true;
        }

        protected async Task OnRowDoubleClick(DataGridRowMouseEventArgs<TaxRates> args)
        {
            if (args.Data == null) return;

            sidebarTitle = $"Editar Impuesto: {args.Data.Name}";
            isNewTaxRate = false;
            selectedTaxRateId = args.Data.TaxRateId;
            sidebarExpanded = true;
        }
        // Cierro el sidebar y recargo si es necesario
        private async Task CloseSidebar(bool reload = false)
        {
            sidebarExpanded = false;

            if (reload)
            {
                await grid.Reload();
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
                        Detail = "Impuesto eliminado correctamente",
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
                        Detail = "No se puede eliminar este impuesto porque está siendo utilizado en una o más facturas.",
                        Duration = 6000
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "No encontrado",
                        Detail = "El impuesto no existe",
                        Duration = 4000
                    });
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Error al eliminar el impuesto",
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

