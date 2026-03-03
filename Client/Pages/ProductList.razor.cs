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
    public partial class ProductList : ComponentBase
    {
        [Inject] protected IProductsService ProductsService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;
        [Inject] protected DialogService DialogService { get; set; } = default!;

        protected RadzenDataGrid<Products> grid = default!;
        protected IEnumerable<Products>? products;
        protected int count;
        protected bool isLoading = true;
        private bool _firstRender = true;

        private bool sidebarExpanded = false;
        private string sidebarTitle = "";
        private bool isNewProduct = true;
        private Guid? selectedProductId = null;

        protected override async Task OnInitializedAsync()
        {
            products = new List<Products>
            {
                new Products { ProductId = Guid.Empty, Name = "", Description = "", CurrentPrice = -1 },
                new Products { ProductId = Guid.Empty, Name = "", Description = "", CurrentPrice = -1 },
                new Products { ProductId = Guid.Empty, Name = "", Description = "", CurrentPrice = -1 },
                new Products { ProductId = Guid.Empty, Name = "", Description = "", CurrentPrice = -1 },
                new Products { ProductId = Guid.Empty, Name = "", Description = "", CurrentPrice = -1 }
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
                var result = await ProductsService.GetProducts(query);

                if (result != null)
                {
                    products = result.Value;
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
            sidebarTitle = "Nuevo Producto";
            isNewProduct = true;
            selectedProductId = null;
            sidebarExpanded = true;
        }

        protected void OnRowDoubleClick(DataGridRowMouseEventArgs<Products> args)
        {
            if (args.Data == null) return;

            sidebarTitle = $"Editar: {args.Data.Name}";
            isNewProduct = false;
            selectedProductId = args.Data.ProductId;
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

        protected async Task DeleteProduct(Guid productId)
        {
            try
            {
                var response = await ProductsService.DeleteProducts(productId);

                if (response.IsSuccessStatusCode)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Producto eliminado correctamente",
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
                        Detail = "No se puede eliminar este producto porque está siendo utilizado en facturas.",
                        Duration = 6000
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "No encontrado",
                        Detail = "El producto no existe",
                        Duration = 4000
                    });
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Error al eliminar el producto",
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
